using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceGenerator : MonoBehaviour
{
    #region Types

    public enum AlignAxis { UpY, DownY, ForwardZ, BackwardZ, RightX, LeftX }

    [System.Serializable]
    public struct SpawnItem
    {
        public GameObject prefab;
        public int count;
        public AlignAxis alignAxis;
        public float offset;
        public bool avoidWater;
        [HideInInspector] public bool initialized;
    }

    struct OceanInfo
    {
        public bool available;
        public Vector3 centre;
        public float radius;
    }

    #endregion

    #region Serialized

    [SerializeField] private ResourceGeneratorConfig config;
    [SerializeField] private string planetTag = "Planet";
    [SerializeField] private SpawnItem[] resources = System.Array.Empty<SpawnItem>();
    [SerializeField] private SpawnItem[] props = System.Array.Empty<SpawnItem>();
    [SerializeField] private float collisionCheckRadius = 1.0f;
    [SerializeField] private Vector3 rotationAdjustment = Vector3.zero;
    [SerializeField] private float waterMargin = 0.05f;
    [SerializeField] private float oceanResolveTimeout = 1f;
    [SerializeField] private int maxAttemptsPerInstance = 64;
    [SerializeField] private Transform planetOverride;

    #endregion

    #region Runtime

    private Transform planet;
    private float planetRadius;
    private OceanInfo ocean;

    #endregion

    #region Unity Callbacks

    void Start() => StartCoroutine(WaitForPlanetGeneration());

    void OnValidate()
    {
        InitDefaults(resources);
        InitDefaults(props);
        SyncPreviewFromConfig();
    }

    #endregion

    #region Configuration API

    public void ConfigureForPlanet(Transform target, ResourceGeneratorConfig cfg)
    {
        planetOverride = target;
        config = cfg;
        SyncPreviewFromConfig();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    #endregion

    #region Generation Flow

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(0.3f);

        FindPlanet();
        if (planet == null) yield break;

        if (AnyItemAvoidsWater()) yield return ResolveOceanInfoRoutine(ActiveOceanResolveTimeout);

        SpawnPrefabs(ActiveResources);
        SpawnPrefabs(ActiveProps);
    }

    private void FindPlanet()
    {
        if (planetOverride)
        {
            planet = planetOverride;
            planetRadius = CalculatePlanetRadius(planet);
            return;
        }

        var planetObject = GameObject.FindGameObjectWithTag(planetTag);
        if (!planetObject) return;

        planet = planetObject.transform;
        planetRadius = CalculatePlanetRadius(planet);
    }

    private float CalculatePlanetRadius(Transform target)
    {
        var renderer = target.GetComponentInChildren<MeshRenderer>();
        if (!renderer) return 0f;

        var b = renderer.bounds;
        return Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
    }

    private void SpawnPrefabs(SpawnItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
            SpawnPrefabInstances(items[i]);
    }

    private void SpawnPrefabInstances(SpawnItem item)
    {
        if (!item.prefab || item.count <= 0) return;

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = item.count * Mathf.Max(1, ActiveMaxAttemptsPerInstance);

        while (spawned < item.count && attempts < maxAttempts)
        {
            attempts++;

            if (!TryGetRandomPointOnPlanet(item.offset, out Vector3 spawnPosition, out Vector3 normal)) continue;
            if (item.avoidWater && ocean.available && IsUnderWater(spawnPosition - normal * item.offset)) continue;
            if (IsPositionOccupied(spawnPosition)) continue;

            var align = Quaternion.FromToRotation(AxisToVector(item.alignAxis), normal);
            var randomAroundNormal = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);
            var finalRotation = randomAroundNormal * align * Quaternion.Euler(ActiveRotationAdjustment);

            Instantiate(item.prefab, spawnPosition, finalRotation, planet);
            spawned++;
        }
    }

    private bool TryGetRandomPointOnPlanet(float offset, out Vector3 spawnPosition, out Vector3 normal)
    {
        var dir = Random.onUnitSphere;
        var rayOrigin = planet.position + dir * (planetRadius + offset);

        if (!Physics.Raycast(rayOrigin, -dir, out var hit, Mathf.Infinity))
        {
            spawnPosition = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        if (hit.collider.transform != planet && !hit.collider.transform.IsChildOf(planet))
        {
            spawnPosition = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        normal = hit.normal.normalized;
        spawnPosition = hit.point + normal * offset;
        return true;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, ActiveCollisionCheckRadius);
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].CompareTag("Resource") || colliders[i].CompareTag("Prop")) return true;

        return false;
    }

    private static Vector3 AxisToVector(AlignAxis axis) =>
        axis switch
        {
            AlignAxis.UpY => Vector3.up,
            AlignAxis.DownY => Vector3.down,
            AlignAxis.ForwardZ => Vector3.forward,
            AlignAxis.BackwardZ => Vector3.back,
            AlignAxis.RightX => Vector3.right,
            AlignAxis.LeftX => Vector3.left,
            _ => Vector3.up
        };

    #endregion

    #region Ocean

    IEnumerator ResolveOceanInfoRoutine(float timeout)
    {
        ResolveOceanInfo();
        if (ocean.available) yield break;

        float t = 0f;
        var step = 0.05f;

        while (t < timeout && !ocean.available)
        {
            yield return new WaitForSeconds(step);
            t += step;
            ResolveOceanInfo();
        }
    }

    private void ResolveOceanInfo()
    {
        ocean = default;

        var generator = planet ? planet.GetComponentInParent<CelestialBodyGenerator>() : null;
        if (generator)
        {
            ocean.centre = generator.transform.position;
            ocean.radius = generator.GetOceanRadius();
            ocean.available = ocean.radius > 0f;
            if (ocean.available) return;
        }

        if (OceanEffect.CurrentOceanMaterial != null)
        {
            var m = OceanEffect.CurrentOceanMaterial;
            var centre = m.GetVector("oceanCentre");
            var radius = m.GetFloat("oceanRadius");
            if (radius > 0f)
            {
                ocean.centre = new Vector3(centre.x, centre.y, centre.z);
                ocean.radius = radius;
                ocean.available = true;
            }
        }
    }

    private bool IsUnderWater(Vector3 worldPoint)
    {
        if (!ocean.available) return false;
        var d = Vector3.Distance(worldPoint, ocean.centre);
        return d <= ActiveOceanRadiusWithMargin;
    }

    private bool AnyItemAvoidsWater()
    {
        var r = ActiveResources;
        for (int i = 0; i < r.Length; i++)
            if (r[i].avoidWater) return true;

        var p = ActiveProps;
        for (int i = 0; i < p.Length; i++)
            if (p[i].avoidWater) return true;

        return false;
    }

    #endregion

    #region Active Config

    private SpawnItem[] ActiveResources => config ? config.resources : resources;
    private SpawnItem[] ActiveProps => config ? config.props : props;

    private float ActiveCollisionCheckRadius => config ? config.collisionCheckRadius : collisionCheckRadius;
    private Vector3 ActiveRotationAdjustment => config ? config.rotationAdjustment : rotationAdjustment;
    private float ActiveWaterMargin => config ? config.waterMargin : waterMargin;
    private float ActiveOceanResolveTimeout => config ? config.oceanResolveTimeout : oceanResolveTimeout;
    private int ActiveMaxAttemptsPerInstance => config ? config.maxAttemptsPerInstance : maxAttemptsPerInstance;

    private float ActiveOceanRadiusWithMargin => ocean.radius + ActiveWaterMargin;

    #endregion

    #region Helpers

    private void InitDefaults(SpawnItem[] items)
    {
        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
            if (!items[i].initialized)
            {
                items[i].avoidWater = true;
                items[i].initialized = true;
            }
    }

    private void SyncPreviewFromConfig()
    {
        if (!config) return;
        resources = config.resources ?? System.Array.Empty<SpawnItem>();
        props = config.props ?? System.Array.Empty<SpawnItem>();
        collisionCheckRadius = config.collisionCheckRadius;
        rotationAdjustment = config.rotationAdjustment;
        waterMargin = config.waterMargin;
        oceanResolveTimeout = config.oceanResolveTimeout;
        maxAttemptsPerInstance = config.maxAttemptsPerInstance;
    }

    #endregion
}