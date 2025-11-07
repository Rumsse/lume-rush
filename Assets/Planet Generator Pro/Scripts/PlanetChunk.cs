using System.Collections.Generic;
using UnityEngine;

namespace PlanetaryRendering.Terrain
{
    /// <summary>
    /// Generates a single cube-to-sphere planet chunk with layered procedural height (FBM, ridged mountains,
    /// mega peaks, valleys, domain warp) and per-vertex biome coloring.
    /// Designed to be driven by a central manager that calls Initialize(...) per chunk.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public class PlanetChunk : MonoBehaviour
    {
        public enum Face { Up = 0, Down = 1, Left = 2, Right = 3, Front = 4, Back = 5 }

        // chunk identity
        private float planetRadius;
        private Face face;
        private int chunkX, chunkY, chunksPerFace;
        private int vertsPerChunkEdge;

        // mesh / material
        private Mesh mesh;
        private MeshCollider meshCollider;
        private Material material;
        private Transform planetTransform;

        // terrain parameters
        private float heightMultiplier;

        // base noise
        private float baseNoiseScale;
        private int baseOctaves;
        private float basePersistence;
        private float baseLacunarity;

        // mountain
        private float mountainNoiseScale;
        private int mountainOctaves;
        private float mountainStrength;
        private float mountainRidgeSharpness;

        // mega mountains
        private float megaNoiseScale;
        private int megaOctaves;
        private float megaStrength;
        private float megaRarity;
        private float megaRarityBlend;
        private float megaPeakSharpness;

        // mega extras
        private float megaClusterScale;
        private float megaHeightCapMultiplier;

        // valleys
        private float valleyNoiseScale;
        private int valleyOctaves;
        private float valleyStrength;
        private float valleyWidth;
        private float valleyBlend;

        // domain warp
        private float warpScale;
        private float warpStrength;
        private int warpOctaves;

        // detail
        private float detailNoiseScale;
        private float detailStrength;

        // biome mask
        private float biomeNoiseScale;
        private int biomeOctaves;
        private float biomePersistence;
        private float biomeLacunarity;
        private float oceanThreshold;
        private float oceanBlend;
        private float mountainThreshold;
        private float mountainBlend;
        private float oceanDepth;

        private int noiseSeed;

        // biome colors and thresholds
        private Color desertColor;
        private Color normalColor;
        private Color snowColor;
        private Color rockColor;
        private float desertLatitudeThreshold;
        private float desertLatitudeBlend;
        private float snowLatitudeThreshold;
        private float snowLatitudeBlend;
        private float snowHeightStart;
        private float snowHeightRange;

        // cliffs
        private float cliffSlopeThresholdDegrees;
        private float cliffColorBlend;

        /// <summary>
        /// World-space center of this chunk (set after mesh generation).
        /// </summary>
        public Vector3 WorldCenter { get; private set; }

        /// <summary>
        /// Initialize the chunk with all necessary parameters. Call from manager before generation.
        /// </summary>
        public void Initialize(
            float radius,
            Face faceIndex,
            int x, int y,
            int chunksPerSide,
            int vertsEdge,
            Material mat,
            Transform planet,

            // terrain / noise
            float heightMult,
            float baseScale, int baseOct, float basePers, float baseLac,
            float mountainScale, int mountainOct, float mountainStr, float mountainSharp,

            // mega
            float megaScale, int megaOct, float megaStr, float megaRarityParam, float megaRarityBlendParam, float megaPeakSharp,

            // mega extras
            float megaClusterScaleParam, float megaHeightCapMultParam,

            // valleys
            float valleyScale, int valleyOct, float valleyStr, float valleyWidthParam, float valleyBlendParam,

            // warp
            float warpScaleParam, float warpStrParam, int warpOct,

            // detail
            float detailScale, float detailStr,

            // biome mask
            float biomeScale, int biomeOct, float biomePers, float biomeLac, float oceanThresh, float oceanBlendParam, float mountainThresh, float mountainBlendParam, float oceanDepthParam,

            int seed,

            // biomes (colors)
            Color desertCol, Color normalCol, Color snowCol, Color rockCol,
            float desertLatThresh, float desertBlend,
            float snowLatThresh, float snowBlend,
            float snowHStart, float snowHRange,

            // cliffs
            float cliffSlopeDeg, float cliffBlend
            )
        {
            // identity
            planetRadius = Mathf.Max(0.0001f, radius);
            face = faceIndex;
            chunkX = x; chunkY = y;
            chunksPerFace = Mathf.Max(1, chunksPerSide);
            vertsPerChunkEdge = Mathf.Max(1, vertsEdge);
            material = mat;
            planetTransform = planet;

            // terrain
            heightMultiplier = Mathf.Max(0f, heightMult);

            baseNoiseScale = Mathf.Max(0.00001f, baseScale);
            baseOctaves = Mathf.Max(1, baseOct);
            basePersistence = Mathf.Clamp01(basePers);
            baseLacunarity = Mathf.Max(0.1f, baseLac);

            mountainNoiseScale = Mathf.Max(0.00001f, mountainScale);
            mountainOctaves = Mathf.Max(1, mountainOct);
            mountainStrength = Mathf.Max(0f, mountainStr);
            mountainRidgeSharpness = Mathf.Max(1f, mountainSharp);

            megaNoiseScale = Mathf.Max(0.00001f, megaScale);
            megaOctaves = Mathf.Max(1, megaOct);
            megaStrength = Mathf.Max(0f, megaStr);
            megaRarity = Mathf.Clamp01(megaRarityParam);
            megaRarityBlend = Mathf.Max(0.0001f, megaRarityBlendParam);
            megaPeakSharpness = Mathf.Max(0.1f, megaPeakSharp);

            megaClusterScale = Mathf.Max(0.00001f, megaClusterScaleParam);
            megaHeightCapMultiplier = Mathf.Max(0.1f, megaHeightCapMultParam);

            valleyNoiseScale = Mathf.Max(0.00001f, valleyScale);
            valleyOctaves = Mathf.Max(1, valleyOct);
            valleyStrength = Mathf.Max(0f, valleyStr);
            valleyWidth = Mathf.Clamp(valleyWidthParam, 0.01f, 0.9f);
            valleyBlend = Mathf.Max(0.0001f, valleyBlendParam);

            warpScale = Mathf.Max(0.00001f, warpScaleParam);
            warpStrength = Mathf.Clamp01(warpStrParam);
            warpOctaves = Mathf.Max(1, warpOct);

            detailNoiseScale = Mathf.Max(0.00001f, detailScale);
            detailStrength = Mathf.Max(0f, detailStr);

            biomeNoiseScale = Mathf.Max(0.00001f, biomeScale);
            biomeOctaves = Mathf.Max(1, biomeOct);
            biomePersistence = Mathf.Clamp01(biomePers);
            biomeLacunarity = Mathf.Max(0.1f, biomeLac);
            oceanThreshold = Mathf.Clamp01(oceanThresh);
            oceanBlend = Mathf.Max(0.0001f, oceanBlendParam);
            mountainThreshold = Mathf.Clamp01(mountainThresh);
            mountainBlend = Mathf.Max(0.0001f, mountainBlendParam);
            oceanDepth = oceanDepthParam;

            noiseSeed = seed;

            // biomes
            desertColor = desertCol;
            normalColor = normalCol;
            snowColor = snowCol;
            rockColor = rockCol;
            desertLatitudeThreshold = Mathf.Clamp01(desertLatThresh);
            desertLatitudeBlend = Mathf.Max(0.0001f, desertBlend);
            snowLatitudeThreshold = Mathf.Clamp01(snowLatThresh);
            snowLatitudeBlend = Mathf.Max(0.0001f, snowBlend);
            snowHeightStart = snowHStart;
            snowHeightRange = Mathf.Max(0.0001f, snowHRange);

            // cliffs
            cliffSlopeThresholdDegrees = Mathf.Max(0f, cliffSlopeDeg);
            cliffColorBlend = Mathf.Clamp01(cliffBlend);

            // components
            var mr = GetComponent<MeshRenderer>();
            if (mr != null && material != null) mr.sharedMaterial = material;

            meshCollider = GetComponent<MeshCollider>();
            mesh = new Mesh
            {
                name = $"chunk_{face}_x{chunkX}_y{chunkY}"
            };

            // assign mesh to filter
            var mf = GetComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            // generate mesh
            GenerateMeshLocalToChunk();

            // assign to collider
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = false;
                meshCollider.isTrigger = false;
                meshCollider.enabled = true;
            }
        }

        private void GenerateMeshLocalToChunk()
        {
            if (planetTransform == null)
            {
                Debug.LogWarning($"[{name}] Planet transform is null — cannot generate chunk.");
                return;
            }

            mesh.Clear();

            int vEdge = vertsPerChunkEdge + 1;
            int vertsPerSideTotal = chunksPerFace * vertsPerChunkEdge + 1;
            float stepGlobal = 2f / (vertsPerSideTotal - 1);

            int vertexCount = vEdge * vEdge;
            int triCount = vertsPerChunkEdge * vertsPerChunkEdge * 6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[triCount];

            // temporary grids for computation
            Vector3[,] worldPts = new Vector3[vEdge, vEdge];
            Vector3[,] dirGrid = new Vector3[vEdge, vEdge];
            float[,] heightGrid = new float[vEdge, vEdge];

            // sample height and directions
            for (int iy = 0; iy < vEdge; iy++)
            {
                int globalY = chunkY * vertsPerChunkEdge + iy;
                float v = -1f + globalY * stepGlobal;
                for (int ix = 0; ix < vEdge; ix++)
                {
                    int globalX = chunkX * vertsPerChunkEdge + ix;
                    float u = -1f + globalX * stepGlobal;

                    Vector3 cubePoint = CubePoint(face, u, v).normalized;

                    float h = EvaluateHeight(cubePoint);
                    heightGrid[ix, iy] = h;
                    float r = planetRadius + h;

                    Vector3 worldPoint = planetTransform.position + planetTransform.rotation * (cubePoint * r);
                    worldPts[ix, iy] = worldPoint;
                    dirGrid[ix, iy] = cubePoint;
                }
            }

            // determine chunk transform using center vertex
            int centerIx = vertsPerChunkEdge / 2;
            int centerIy = vertsPerChunkEdge / 2;
            Vector3 centerWorld = worldPts[centerIx, centerIy];
            transform.position = centerWorld;
            transform.rotation = planetTransform.rotation;
            WorldCenter = transform.position;

            // fill vertices & uvs (local positions relative to this.transform)
            int vi = 0;
            for (int iy = 0; iy < vEdge; iy++)
            {
                for (int ix = 0; ix < vEdge; ix++, vi++)
                {
                    Vector3 local = worldPts[ix, iy] - transform.position;
                    vertices[vi] = local;

                    float uNorm = (float)ix / (vEdge - 1);
                    float vNorm = (float)iy / (vEdge - 1);
                    uvs[vi] = new Vector2(uNorm, vNorm);
                }
            }

            // build triangles (two per quad)
            int ti = 0;
            for (int y = 0; y < vertsPerChunkEdge; y++)
            {
                for (int x = 0; x < vertsPerChunkEdge; x++)
                {
                    int i0 = y * vEdge + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + vEdge;
                    int i3 = i2 + 1;

                    // triangle 1
                    triangles[ti++] = i0;
                    triangles[ti++] = i2;
                    triangles[ti++] = i1;

                    // triangle 2
                    triangles[ti++] = i1;
                    triangles[ti++] = i2;
                    triangles[ti++] = i3;
                }
            }

            // set arrays
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // ensure triangle winding faces outward (world space)
            FixTriangleWindingIfNeeded(mesh, planetTransform.position);

            // compute normals pointing away from planet center
            Vector3[] localVerts = mesh.vertices;
            Vector3[] normals = new Vector3[localVerts.Length];
            for (int i = 0; i < localVerts.Length; i++)
            {
                Vector3 worldV = transform.TransformPoint(localVerts[i]);
                normals[i] = (worldV - planetTransform.position).normalized;
            }
            mesh.normals = normals;

            // compute vertex colors (biomes + cliffs)
            Color[] colors = new Color[localVerts.Length];
            int idx = 0;
            for (int iy = 0; iy < vEdge; iy++)
            {
                for (int ix = 0; ix < vEdge; ix++, idx++)
                {
                    Vector3 dir = dirGrid[ix, iy];
                    float latitude = Mathf.Abs(dir.y);

                    float desertT = SmoothStepRange(latitude, desertLatitudeThreshold - desertLatitudeBlend, desertLatitudeThreshold + desertLatitudeBlend);
                    float desertFactor = 1f - desertT;
                    float snowLatT = SmoothStepRange(latitude, snowLatitudeThreshold - snowLatitudeBlend, snowLatitudeThreshold + snowLatitudeBlend);

                    Color baseBiome = Color.Lerp(normalColor, desertColor, Mathf.Clamp01(desertFactor));
                    baseBiome = Color.Lerp(baseBiome, snowColor, Mathf.Clamp01(snowLatT));

                    float h = heightGrid[ix, iy];
                    float snowH = SmoothStepRange(h, snowHeightStart, snowHeightStart + snowHeightRange);
                    Color finalColor = Color.Lerp(baseBiome, snowColor, snowH);

                    // steepness by neighboring heights (N,S,E,W)
                    float maxSlopeDeg = 0f;
                    float centerH = h;

                    if (ix > 0)
                    {
                        float dh = centerH - heightGrid[ix - 1, iy];
                        float horizontal = DistanceOnSphere(dir, dirGrid[ix - 1, iy]);
                        float slopeRad = Mathf.Atan2(Mathf.Abs(dh), horizontal);
                        maxSlopeDeg = Mathf.Max(maxSlopeDeg, slopeRad * Mathf.Rad2Deg);
                    }
                    if (ix < vEdge - 1)
                    {
                        float dh = centerH - heightGrid[ix + 1, iy];
                        float horizontal = DistanceOnSphere(dir, dirGrid[ix + 1, iy]);
                        float slopeRad = Mathf.Atan2(Mathf.Abs(dh), horizontal);
                        maxSlopeDeg = Mathf.Max(maxSlopeDeg, slopeRad * Mathf.Rad2Deg);
                    }
                    if (iy > 0)
                    {
                        float dh = centerH - heightGrid[ix, iy - 1];
                        float horizontal = DistanceOnSphere(dir, dirGrid[ix, iy - 1]);
                        float slopeRad = Mathf.Atan2(Mathf.Abs(dh), horizontal);
                        maxSlopeDeg = Mathf.Max(maxSlopeDeg, slopeRad * Mathf.Rad2Deg);
                    }
                    if (iy < vEdge - 1)
                    {
                        float dh = centerH - heightGrid[ix, iy + 1];
                        float horizontal = DistanceOnSphere(dir, dirGrid[ix, iy + 1]);
                        float slopeRad = Mathf.Atan2(Mathf.Abs(dh), horizontal);
                        maxSlopeDeg = Mathf.Max(maxSlopeDeg, slopeRad * Mathf.Rad2Deg);
                    }

                    if (maxSlopeDeg >= cliffSlopeThresholdDegrees)
                    {
                        float t = Mathf.InverseLerp(cliffSlopeThresholdDegrees, cliffSlopeThresholdDegrees + 40f, maxSlopeDeg);
                        t = Mathf.Clamp01(t) * cliffColorBlend;
                        finalColor = Color.Lerp(finalColor, rockColor, t);
                    }

                    colors[idx] = finalColor;
                }
            }

            mesh.colors = colors;

            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }

        /// <summary>
        /// Fix triangle winding if a triangle's normal points inward relative to planet center.
        /// </summary>
        private void FixTriangleWindingIfNeeded(Mesh m, Vector3 planetCenter)
        {
            int[] tri = m.triangles;
            Vector3[] verts = m.vertices;

            for (int t = 0; t < tri.Length; t += 3)
            {
                int ia = tri[t + 0];
                int ib = tri[t + 1];
                int ic = tri[t + 2];

                Vector3 w0 = transform.TransformPoint(verts[ia]);
                Vector3 w1 = transform.TransformPoint(verts[ib]);
                Vector3 w2 = transform.TransformPoint(verts[ic]);

                Vector3 faceNormal = Vector3.Cross(w1 - w0, w2 - w0);
                float mag = faceNormal.magnitude;
                if (mag < 1e-6f) continue;
                faceNormal /= mag;

                Vector3 triCenter = (w0 + w1 + w2) / 3f;
                Vector3 outward = (triCenter - planetCenter).normalized;

                if (Vector3.Dot(faceNormal, outward) < 0f)
                {
                    // swap to flip winding
                    tri[t + 1] = ic;
                    tri[t + 2] = ib;
                }
            }

            m.triangles = tri;
        }

        // --- Utilities: noise, warp, cube mapping, etc.

        private float DistanceOnSphere(Vector3 aDir, Vector3 bDir)
        {
            float dot = Mathf.Clamp(Vector3.Dot(aDir.normalized, bDir.normalized), -1f, 1f);
            float angle = Mathf.Acos(dot); // radians
            return angle * planetRadius;
        }

        private float EvaluateHeight(Vector3 dirNormalized)
        {
            // domain warp
            Vector3 warped = DomainWarp(dirNormalized, warpScale, warpStrength, warpOctaves, noiseSeed + 9001);

            // biome mask (low frequency)
            float bioRaw = FBM3D(warped * biomeNoiseScale, biomeOctaves, biomePersistence, biomeLacunarity, noiseSeed + 999);
            float bio01 = Mathf.Clamp01(bioRaw * 0.5f + 0.5f);

            // zone weights
            float oceanW = 1f - SmoothStepRange(bio01, oceanThreshold - oceanBlend, oceanThreshold + oceanBlend);
            float mountainW = SmoothStepRange(bio01, mountainThreshold - mountainBlend, mountainThreshold + mountainBlend);
            float plainsW = SmoothStepRange(bio01, oceanThreshold + oceanBlend, mountainThreshold - mountainBlend);

            float sum = oceanW + plainsW + mountainW;
            if (sum <= 0f) { plainsW = 1f; oceanW = 0f; mountainW = 0f; sum = 1f; }
            oceanW /= sum; plainsW /= sum; mountainW /= sum;

            // base small hills
            float baseFbm = FBM3D(warped * baseNoiseScale, baseOctaves, basePersistence, baseLacunarity, noiseSeed); // [-1..1]
            float plainsHeight = baseFbm * heightMultiplier * 0.18f;

            // mountains (ridged)
            float mfbm = FBM3D(warped * mountainNoiseScale, mountainOctaves, basePersistence, baseLacunarity, noiseSeed + 1337);
            float ridged = 1f - Mathf.Abs(mfbm);
            ridged = Mathf.Pow(Mathf.Clamp01(ridged), mountainRidgeSharpness);
            float mountainHeight = ridged * mountainStrength * heightMultiplier;

            // mega peaks
            float megaRaw = FBM3D(warped * megaNoiseScale, megaOctaves, basePersistence, baseLacunarity, noiseSeed + 4242);
            float mega01 = Mathf.Clamp01(megaRaw * 0.5f + 0.5f);
            float megaMask = SmoothStepRange(mega01, megaRarity - megaRarityBlend, megaRarity + megaRarityBlend);
            megaMask = Mathf.Pow(Mathf.Clamp01(megaMask), 1f + megaPeakSharpness);

            float megaClusterRaw = FBM3D(warped * megaClusterScale, Mathf.Max(1, megaOctaves - 1), Mathf.Clamp01(basePersistence), baseLacunarity, noiseSeed + 202020);
            float megaCluster01 = Mathf.Clamp01(megaClusterRaw * 0.5f + 0.5f);
            float megaClusterFactor = Mathf.Pow(megaCluster01, 1.2f);

            float megaDetail = FBM3D(warped * (megaNoiseScale * 5.5f), Mathf.Max(1, megaOctaves - 1), basePersistence, baseLacunarity, noiseSeed + 52525);
            float megaRidged = 1f - Mathf.Abs(megaDetail);
            megaRidged = Mathf.Pow(Mathf.Clamp01(megaRidged), 2.0f + megaPeakSharpness * 0.5f);

            float megaContribution = megaRidged * megaMask * megaStrength * heightMultiplier * Mathf.Lerp(1f, megaHeightCapMultiplier, megaClusterFactor);

            // detail
            float detailFbm = FBM3D(warped * detailNoiseScale, 2, basePersistence, baseLacunarity, noiseSeed + 777);
            float detailContribution = detailFbm * detailStrength * heightMultiplier;

            // valleys
            float vfield = Mathf.Abs(FBM3D(warped * valleyNoiseScale, valleyOctaves, 0.5f, 2f, noiseSeed + 90210));
            float valleyMask = 1f - SmoothStepRange(vfield, valleyWidth, valleyWidth + valleyBlend);
            valleyMask = Mathf.Pow(Mathf.Clamp01(valleyMask), 1.5f);
            float valleyCut = valleyMask * valleyStrength * heightMultiplier;

            // ocean baseline
            float oceanBase = oceanDepth + baseFbm * (0.05f * heightMultiplier);

            float mountainZoneHeight = mountainHeight + megaContribution + detailContribution - valleyCut;
            mountainZoneHeight = Mathf.Max(mountainZoneHeight, -0.6f * heightMultiplier);

            float plainsZoneHeight = plainsHeight + detailContribution * 0.4f - valleyCut * 0.3f;
            plainsZoneHeight = Mathf.Max(plainsZoneHeight, -0.4f * heightMultiplier);

            float oceanZoneHeight = Mathf.Min(oceanBase, plainsZoneHeight * 0.25f);

            float finalHeight = oceanW * oceanZoneHeight + plainsW * plainsZoneHeight + mountainW * mountainZoneHeight;

            return finalHeight;
        }

        private float SmoothStepRange(float v, float a, float b)
        {
            if (b <= a) return v >= b ? 1f : 0f;
            float t = Mathf.Clamp01((v - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        private Vector3 DomainWarp(Vector3 p, float scale, float strength, int octaves, int seed)
        {
            Vector3 o1 = NoiseVec3(p * scale, octaves, seed);
            Vector3 o2 = NoiseVec3(new Vector3(p.z, p.x, p.y) * (scale * 1.37f), octaves, seed + 123);
            Vector3 warp = (o1 * 0.6f + o2 * 0.4f) * strength;
            Vector3 q = (p + warp).normalized;
            return q;
        }

        private Vector3 NoiseVec3(Vector3 p, int octaves, int seed)
        {
            float x = FBM3D(p + new Vector3(17.1f, 31.7f, 12.3f), octaves, 0.5f, 2f, seed) * 0.5f;
            float y = FBM3D(p + new Vector3(27.5f, 9.8f, 44.2f), octaves, 0.5f, 2f, seed + 1) * 0.5f;
            float z = FBM3D(p + new Vector3(5.9f, 61.3f, 7.7f), octaves, 0.5f, 2f, seed + 2) * 0.5f;
            return new Vector3(x, y, z);
        }

        private float FBM3D(Vector3 p, int octaves, float persistence, float lacunarity, int seedOffset)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float sum = 0f;
            float ampSum = 0f;
            Vector3 seedOffsetV = new Vector3(seedOffset * 0.1234f, seedOffset * 0.2345f, seedOffset * 0.3456f);
            for (int i = 0; i < octaves; i++)
            {
                Vector3 sp = p * frequency + seedOffsetV;
                float s = Perlin3D(sp);
                sum += s * amplitude;
                ampSum += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            if (ampSum == 0f) return 0f;
            return Mathf.Clamp(sum / ampSum, -1f, 1f);
        }

        private float Perlin3D(Vector3 p)
        {
            float a = Mathf.PerlinNoise(p.x, p.y);
            float b = Mathf.PerlinNoise(p.y, p.z);
            float c = Mathf.PerlinNoise(p.z, p.x);
            float v = (a + b + c) / 3f;
            return v * 2f - 1f;
        }

        private Vector3 CubePoint(Face faceIdx, float u, float v)
        {
            switch (faceIdx)
            {
                case Face.Up: return new Vector3(u, 1f, -v);
                case Face.Down: return new Vector3(u, -1f, v);
                case Face.Left: return new Vector3(-1f, v, u);
                case Face.Right: return new Vector3(1f, v, -u);
                case Face.Front: return new Vector3(u, v, 1f);
                default: return new Vector3(-u, v, -1f); // Back
            }
        }

        /// <summary>
        /// Helper to get an approximate diagonal size (for LOD / culling decisions).
        /// </summary>
        public float GetApproxDiagonal()
        {
            var mf = GetComponent<MeshFilter>();
            if (mf != null && mf.mesh != null)
                return mf.mesh.bounds.extents.magnitude * 2f;
            return 10f;
        }

        // Optional: keep the chunk's mesh when disabled
        private void OnDisable()
        {
            // keep mesh data; if you want to free memory uncomment the following:
            // if (mesh != null) { mesh.Clear(); }
        }
    }
}
