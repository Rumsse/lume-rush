using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlanetaryRendering.Terrain;

namespace PlanetaryRendering.Generation
{
    /// <summary>
    /// Procedural planet generator that creates cube-to-sphere chunks and manages LOD / culling.
    /// Clean, publication-ready version with English-only tooltips and namespace.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlanetGenerator : MonoBehaviour
    {
        [Header("Planet")]
        public float radius = 5000f;
        public Material planetMaterial;

        [Header("Chunking")]
        public int vertsPerChunkEdge = 16;
        public int chunksPerFace = 8;

        [Header("Terrain - global")]
        public float heightMultiplier = 220f;

        [Header("Base noise (hills / rolling terrain)")]
        public float baseNoiseScale = 3.0f;
        public int baseOctaves = 4;
        [Range(0f, 1f)] public float basePersistence = 0.5f;
        public float baseLacunarity = 2f;

        [Header("Mountains (ridged)")]
        public float mountainNoiseScale = 20f;
        public int mountainOctaves = 3;
        public float mountainStrength = 1.25f;
        public float mountainRidgeSharpness = 2.4f;

        [Header("Mega mountains (rare huge peaks)")]
        public float megaNoiseScale = 0.5f;
        public int megaOctaves = 3;
        public float megaStrength = 4.5f;
        [Range(0f, 1f)] public float megaRarity = 0.965f;
        public float megaRarityBlend = 0.035f;
        public float megaPeakSharpness = 4f;
        [Tooltip("Controls clustering/size of mega blobs (lower = larger blobs).")]
        public float megaClusterScale = 0.06f;
        [Tooltip("Multiplier for mega peak cap (applied to megaStrength * heightMultiplier).")]
        public float megaHeightCapMultiplier = 1.8f;

        [Header("Valleys / troughs")]
        public float valleyNoiseScale = 1.4f;
        public int valleyOctaves = 2;
        public float valleyStrength = 0.65f;
        [Range(0.01f, 0.5f)] public float valleyWidth = 0.18f;
        public float valleyBlend = 0.12f;

        [Header("Domain warp (natural shapes)")]
        public float warpScale = 0.6f;
        public float warpStrength = 0.25f;
        public int warpOctaves = 2;

        [Header("Detail")]
        public float detailNoiseScale = 80f;
        public float detailStrength = 0.08f;

        [Header("Global biome mask (ocean / plains / mountains)")]
        public float biomeNoiseScale = 0.35f;
        public int biomeOctaves = 3;
        [Range(0f, 1f)] public float biomePersistence = 0.5f;
        public float biomeLacunarity = 2f;
        [Range(0f, 1f)] public float oceanThreshold = 0.25f;
        public float oceanBlend = 0.05f;
        [Range(0f, 1f)] public float mountainThreshold = 0.85f;
        public float mountainBlend = 0.05f;
        public float oceanDepth = -150f;

        [Header("Noise")]
        public int noiseSeed = 1337;

        [Header("Biomes (colors)")]
        public Color desertColor = new Color(0.9849057f, 0.2339151f, 1f);
        public Color normalColor = new Color(0.102f, 0.6f, 0.12f);
        public Color snowColor = Color.white;
        public Color rockColor = new Color(0.45f, 0.42f, 0.38f);
        [Range(0f, 1f)] public float desertLatitudeThreshold = 0.25f;
        public float desertLatitudeBlend = 0.08f;
        [Range(0f, 1f)] public float snowLatitudeThreshold = 0.8f;
        public float snowLatitudeBlend = 0.08f;
        public float snowHeightStart = 0.4f * 200f;
        public float snowHeightRange = 0.6f * 200f;

        [Header("Cliffs / slopes")]
        public float cliffSlopeThresholdDegrees = 35f;
        [Range(0f, 1f)] public float cliffColorBlend = 0.95f;

        [Header("Generation / Safety")]
        public bool useProxySphereDuringGeneration = true;
        public float proxyExtraHeight = 10f;

        [Header("Culling / Performance")]
        public Camera mainCamera;
        public Transform playerTransform;
        public float renderDistance = 2000f;
        public float colliderDistance = 300f;
        public int extraColliderChunks = 1;
        public float cullInterval = 0.25f;

        [Header("Startup")]
        public bool autoPlacePlayerOnSurface = true;
        public float playerSurfaceOffset = 1.0f;

        // internal
        private PlanetChunk[,,] chunks;
        private const int FACE_COUNT = 6;
        private bool cullRunning = false;
        private GameObject parentGO;
        private GameObject proxySphereGO;

        private struct ChunkInfo { public int face; public int x; public int y; public float dist; }

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (playerTransform == null && mainCamera != null) playerTransform = mainCamera.transform;

            if (snowHeightStart <= 0f) snowHeightStart = 0.4f * heightMultiplier;
            if (snowHeightRange <= 0f) snowHeightRange = 0.6f * heightMultiplier;

            StartCoroutine(GenerateAllChunksCoroutine());
        }

        IEnumerator GenerateAllChunksCoroutine()
        {
            // clean existing
            if (parentGO != null) Destroy(parentGO);
            chunks = new PlanetChunk[FACE_COUNT, chunksPerFace, chunksPerFace];

            parentGO = new GameObject("PlanetChunks");
            parentGO.transform.SetParent(transform, false);
            parentGO.transform.localPosition = Vector3.zero;
            parentGO.transform.localRotation = Quaternion.identity;

            if (useProxySphereDuringGeneration) CreateProxySphere();

            Vector3 playerPos = playerTransform != null ? playerTransform.position : (transform.position + Vector3.forward * (radius + 10f));
            var list = new List<ChunkInfo>(FACE_COUNT * chunksPerFace * chunksPerFace);

            // compute priorities (distance from player)
            for (int f = 0; f < FACE_COUNT; f++)
                for (int y = 0; y < chunksPerFace; y++)
                    for (int x = 0; x < chunksPerFace; x++)
                    {
                        float chunkSizeNorm = 2f / chunksPerFace;
                        float startU = -1f + x * chunkSizeNorm;
                        float startV = -1f + y * chunkSizeNorm;
                        float midU = startU + chunkSizeNorm * 0.5f;
                        float midV = startV + chunkSizeNorm * 0.5f;

                        Vector3 centerCube = CubePoint((PlanetChunk.Face)f, midU, midV).normalized;
                        Vector3 centerWorld = transform.position + transform.rotation * (centerCube * radius);
                        float d = Vector3.Distance(centerWorld, playerPos);

                        list.Add(new ChunkInfo { face = f, x = x, y = y, dist = d });
                    }

            list.Sort((a, b) => a.dist.CompareTo(b.dist));

            int created = 0;
            foreach (var it in list)
            {
                int f = it.face, x = it.x, y = it.y;
                GameObject go = new GameObject($"chunk_f{f}_x{x}_y{y}");
                go.transform.SetParent(parentGO.transform, false);

                PlanetChunk chunk = go.AddComponent<PlanetChunk>();
                chunk.Initialize(
                    radius,
                    (PlanetChunk.Face)f,
                    x, y,
                    chunksPerFace,
                    vertsPerChunkEdge,
                    planetMaterial,
                    transform,
                    // terrain / noise
                    heightMultiplier,
                    baseNoiseScale, baseOctaves, basePersistence, baseLacunarity,
                    mountainNoiseScale, mountainOctaves, mountainStrength, mountainRidgeSharpness,
                    // mega
                    megaNoiseScale, megaOctaves, megaStrength, megaRarity, megaRarityBlend, megaPeakSharpness,
                    // mega extras
                    megaClusterScale, megaHeightCapMultiplier,
                    // valleys
                    valleyNoiseScale, valleyOctaves, valleyStrength, valleyWidth, valleyBlend,
                    // warp
                    warpScale, warpStrength, warpOctaves,
                    // detail
                    detailNoiseScale, detailStrength,
                    // biome mask
                    biomeNoiseScale, biomeOctaves, biomePersistence, biomeLacunarity, oceanThreshold, oceanBlend, mountainThreshold, mountainBlend, oceanDepth,
                    // seed
                    noiseSeed,
                    // biomes (colors)
                    desertColor, normalColor, snowColor, rockColor,
                    desertLatitudeThreshold, desertLatitudeBlend,
                    snowLatitudeThreshold, snowLatitudeBlend,
                    snowHeightStart, snowHeightRange,
                    // cliffs
                    cliffSlopeThresholdDegrees, cliffColorBlend
                );

                var rend = go.GetComponent<MeshRenderer>();
                if (rend != null) rend.enabled = true;
                var mc = go.GetComponent<MeshCollider>();
                if (mc != null) mc.enabled = true;

                chunks[f, x, y] = chunk;

                created++;
                if ((created % 8) == 0) yield return null;
            }

            // remove proxy and sync physics
            if (proxySphereGO != null)
            {
                Destroy(proxySphereGO);
                proxySphereGO = null;
                yield return null;
                Physics.SyncTransforms();
            }

            if (autoPlacePlayerOnSurface && playerTransform != null)
            {
                TryPlacePlayerByRaycast();
                yield return null;
            }

            UpdateVisibilityImmediate();

            if (!cullRunning) StartCoroutine(CullLoop());
        }

        IEnumerator CullLoop()
        {
            cullRunning = true;
            while (true)
            {
                UpdateVisibilityImmediate();
                yield return new WaitForSeconds(cullInterval);
            }
        }

        void CreateProxySphere()
        {
            if (proxySphereGO != null) return;

            proxySphereGO = new GameObject("PlanetProxyCollider");
            proxySphereGO.transform.SetParent(transform, false);
            proxySphereGO.transform.localPosition = Vector3.zero;
            proxySphereGO.transform.localRotation = Quaternion.identity;
            SphereCollider sc = proxySphereGO.AddComponent<SphereCollider>();
            sc.isTrigger = false;
            sc.radius = radius + Mathf.Abs(heightMultiplier) + proxyExtraHeight;
        }

        void TryPlacePlayerByRaycast()
        {
            if (playerTransform == null) return;

            Vector3 planetCenter = transform.position;
            Vector3 playerPos = playerTransform.position;
            Vector3 radialDir = (playerPos - planetCenter).normalized;
            if (radialDir.sqrMagnitude < 1e-6f) radialDir = transform.up;

            float far = radius + Mathf.Abs(heightMultiplier) * 3f + 1000f;
            Vector3 origin = planetCenter + radialDir * far;
            Vector3 dir = -radialDir;
            float maxDist = far * 2f;

            Collider[] playerCols = playerTransform.GetComponentsInChildren<Collider>(true);
            bool[] prevEnabled = new bool[playerCols.Length];
            for (int i = 0; i < playerCols.Length; i++) { prevEnabled[i] = playerCols[i].enabled; playerCols[i].enabled = false; }

            Physics.SyncTransforms();

            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, maxDist))
            {
                Vector3 outward = (hit.point - planetCenter).normalized;
                Vector3 targetPos = hit.point + outward * playerSurfaceOffset;
                PlacePlayerAtPosition(targetPos);
            }
            else
            {
                Vector3 fallbackPos = planetCenter + radialDir * (radius + playerSurfaceOffset);
                PlacePlayerAtPosition(fallbackPos);
            }

            for (int i = 0; i < playerCols.Length; i++) playerCols[i].enabled = prevEnabled[i];
            Physics.SyncTransforms();
        }

        void PlacePlayerAtPosition(Vector3 pos)
        {
            if (playerTransform == null) return;

            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                playerTransform.position = pos;
                StartCoroutine(ReenableCharacterControllerNextFrame(cc));
                return;
            }

            var rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = pos;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                return;
            }

            playerTransform.position = pos;
        }

        IEnumerator ReenableCharacterControllerNextFrame(CharacterController cc)
        {
            yield return null;
            if (cc != null) cc.enabled = true;
        }

        /// <summary>
        /// Update chunk renderer / collider visibility based on camera frustum and distances.
        /// </summary>
        void UpdateVisibilityImmediate()
        {
            if (chunks == null || mainCamera == null) return;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;

            for (int f = 0; f < FACE_COUNT; f++)
                for (int y = 0; y < chunksPerFace; y++)
                    for (int x = 0; x < chunksPerFace; x++)
                    {
                        var chunk = chunks[f, x, y];
                        if (chunk == null) continue;
                        Renderer r = chunk.GetComponent<MeshRenderer>();
                        Collider col = chunk.GetComponent<MeshCollider>();
                        Bounds bounds = (r != null) ? r.bounds : new Bounds(chunk.WorldCenter, Vector3.one * 10f);
                        bool inFrustum = GeometryUtility.TestPlanesAABB(planes, bounds);
                        float distToPlayer = Vector3.Distance(playerPos, chunk.WorldCenter);
                        bool withinRenderDist = distToPlayer <= renderDistance;
                        bool shouldRender = inFrustum && withinRenderDist;
                        if (r != null && r.enabled != shouldRender) r.enabled = shouldRender;

                        bool shouldHaveCollider = distToPlayer <= colliderDistance;
                        if (!shouldHaveCollider && playerTransform != null)
                        {
                            float chunkDiag = chunk.GetApproxDiagonal();
                            if (distToPlayer <= colliderDistance + chunkDiag * extraColliderChunks) shouldHaveCollider = true;
                        }
                        if (col != null && col.enabled != shouldHaveCollider) col.enabled = shouldHaveCollider;
                    }
        }

        private Vector3 CubePoint(PlanetChunk.Face faceIdx, float u, float v)
        {
            switch (faceIdx)
            {
                case PlanetChunk.Face.Up:    return new Vector3(u, 1f, -v);
                case PlanetChunk.Face.Down:  return new Vector3(u, -1f, v);
                case PlanetChunk.Face.Left:  return new Vector3(-1f, v, u);
                case PlanetChunk.Face.Right: return new Vector3(1f, v, -u);
                case PlanetChunk.Face.Front: return new Vector3(u, v, 1f);
                default:                     return new Vector3(-u, v, -1f); // Back
            }
        }

        /// <summary>
        /// Quick helper to get a surface position in a given direction.
        /// </summary>
        public Vector3 GetSurfacePositionByDirection(Vector3 directionNormalized, float offset = 0f)
        {
            if (directionNormalized.sqrMagnitude < 0.0001f) directionNormalized = transform.up;
            directionNormalized = directionNormalized.normalized;
            return transform.position + directionNormalized * (radius + offset);
        }

        void OnValidate()
        {
            vertsPerChunkEdge = Mathf.Max(1, vertsPerChunkEdge);
            chunksPerFace = Mathf.Max(1, chunksPerFace);
            radius = Mathf.Max(0.001f, radius);
            heightMultiplier = Mathf.Max(0f, heightMultiplier);
            baseNoiseScale = Mathf.Max(0.00001f, baseNoiseScale);
            mountainNoiseScale = Mathf.Max(0.00001f, mountainNoiseScale);
            megaNoiseScale = Mathf.Max(0.00001f, megaNoiseScale);
            valleyNoiseScale = Mathf.Max(0.00001f, valleyNoiseScale);
            warpScale = Mathf.Max(0.00001f, warpScale);
            detailNoiseScale = Mathf.Max(0.00001f, detailNoiseScale);
            biomeNoiseScale = Mathf.Max(0.00001f, biomeNoiseScale);
            renderDistance = Mathf.Max(0f, renderDistance);
            colliderDistance = Mathf.Max(0f, colliderDistance);
            cullInterval = Mathf.Max(0.01f, cullInterval);
            snowHeightRange = Mathf.Max(0.0001f, snowHeightRange);
        }

        void OnDisable()
        {
            cullRunning = false;
            // optionally stop coroutines if needed
            StopAllCoroutines();
        }

        void OnDestroy()
        {
            cullRunning = false;
        }
    }
}
