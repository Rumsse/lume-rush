using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawnerManager : MonoBehaviour
{
    #region Inspector
    public List<CelestialBodySettings> presets = new List<CelestialBodySettings>();
    public int spawnCount = 5;
    public float spawnRadius = 200f;
    public Vector2 scaleRange = new Vector2(5f, 20f);
    public bool randomizeSeed = true;
    public int seedMin = 0;
    public int seedMax = int.MaxValue - 1;
    public bool parentUnderThis = true;

    [Header("Gravity / Tag / Layer")]
    public bool addGravityArea = true;
    [Min(0.01f)] public float gravityAreaMultiplier = 1.5f; // sphere collider radius in local (will be scaled by planet scale)
    public bool gravityAreaIsTrigger = true;
    public string gravityComponentTypeName = "GravityAreaPoint";
    public string planetTag = "Planet";
    public string planetLayerName = "Planet";
    #endregion

    void Start() => SpawnAll();

    #region Spawning
    public void SpawnAll()
    {
        if (presets == null || presets.Count == 0 || spawnCount <= 0) return;

        for (int i = 0; i < spawnCount; i++)
            SpawnOne(i);
    }

    void SpawnOne(int index)
    {
        var preset = presets[Random.Range(0, presets.Count)];
        if (!preset) return;

        var runtimeSettings = Instantiate(preset);
        if (runtimeSettings.shape) runtimeSettings.shape = Instantiate(runtimeSettings.shape);
        if (runtimeSettings.shading) runtimeSettings.shading = Instantiate(runtimeSettings.shading);

        if (randomizeSeed && runtimeSettings.shape)
            runtimeSettings.shape.seed = Random.Range(seedMin, seedMax);

        var go = new GameObject($"Planet_{index}");
        if (parentUnderThis) go.transform.parent = transform;

        go.transform.position = Random.onUnitSphere * spawnRadius;
        float s = Random.Range(scaleRange.x, scaleRange.y);
        go.transform.localScale = Vector3.one * s;

        var generator = go.AddComponent<CelestialBodyGenerator>();
        generator.resolutionSettings = new CelestialBodyGenerator.ResolutionSettings();
        generator.body = runtimeSettings;

        // tag & layer on planet root
        try { go.tag = planetTag; } catch { Debug.LogWarning($"Tag '{planetTag}' not found. Set tag manually."); }
        int layer = LayerMask.NameToLayer(planetLayerName);
        if (layer >= 0) go.layer = layer;
        else Debug.LogWarning($"Layer '{planetLayerName}' not found. Set layer manually.");

        // add child gravity area as SphereCollider + optional gravity script
        if (addGravityArea)
        {
            var gravityGO = new GameObject("Gravity Area");
            gravityGO.transform.parent = go.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.transform.localRotation = Quaternion.identity;
            gravityGO.transform.localScale = Vector3.one * 1.75f;

            var sc = gravityGO.AddComponent<SphereCollider>();
            sc.isTrigger = gravityAreaIsTrigger;
            sc.radius = gravityAreaMultiplier;

            // ensure child uses same layer as planet
            gravityGO.layer = go.layer;

            // then reset child layer to Default so only parent keeps Planet layer
            gravityGO.layer = 0;

            // try to attach gravity component by type name (reflection fallback)
            var type = System.Type.GetType(gravityComponentTypeName) ?? System.Type.GetType($"{gravityComponentTypeName},Assembly-CSharp");
            if (type != null && typeof(Component).IsAssignableFrom(type))
                gravityGO.AddComponent(type);
            else
                Debug.LogWarning($"Gravity component '{gravityComponentTypeName}' not found. Add it to the project or change the name in the spawner.");
        }

        StartCoroutine(EnsureGeneratorVisible(generator));

        if (index == 0)
        {
            var cam = Object.FindFirstObjectByType<ThrirdPersonPlanetCamera>();
            if (cam != null)
                cam.SetJumpLookTarget(go.transform);
        }
    }

    IEnumerator EnsureGeneratorVisible(CelestialBodyGenerator generator)
    {
        const int maxFrames = 30;
        int frame = 0;
        Transform terrainChild = null;
        while (frame++ < maxFrames)
        {
            if (generator == null) yield break;
            terrainChild = generator.transform.Find("Terrain Mesh");
            if (terrainChild != null)
            {
                var mf = terrainChild.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh == null)
                    generator.SetLOD(0);
                yield break;
            }
            yield return null;
        }

        if (generator != null)
            generator.SetLOD(0);
    }
    #endregion
}