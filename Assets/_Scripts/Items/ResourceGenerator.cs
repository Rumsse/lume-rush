using System.Collections;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour
{
    #region Types

    public enum AlignAxis
    {
        UpY, DownY,
        ForwardZ, BackwardZ,
        RightX, LeftX
    }

    [System.Serializable]
    public struct SpawnItem
    {
        public GameObject prefab;
        public int count;
        public AlignAxis alignAxis;
        public bool useCustomOffset;
        public float offset;
    }

    #endregion


    #region Config

    [SerializeField] private string planetTag = "Planet";

    [Header("Resources")]
    [SerializeField] private SpawnItem[] resources;

    [Header("Props")]
    [SerializeField] private SpawnItem[] props;

    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float collisionCheckRadius = 1.0f;
    [SerializeField] private Vector3 rotationAdjustment = Vector3.zero;

    #endregion


    #region State

    private Transform planet;
    private float planetRadius;

    #endregion


    #region Lifecycle

    void Start() => StartCoroutine(WaitForPlanetGeneration());

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(0.1f);

        FindPlanet();
        if (planet == null) yield break;

        SpawnPrefabs(resources);
        SpawnPrefabs(props);
    }

    #endregion


    #region Generation

    private void FindPlanet()
    {
        var planetObject = GameObject.FindGameObjectWithTag(planetTag);
        if (planetObject == null) return;

        planet = planetObject.transform;
        planetRadius = CalculatePlanetRadius(planet);
    }

    private float CalculatePlanetRadius(Transform target)
    {
        var renderer = target.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return 0f;

        var bounds = renderer.bounds;
        return Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
    }

    private void SpawnPrefabs(SpawnItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
            SpawnPrefabInstances(items[i]);
    }

    private void SpawnPrefabInstances(SpawnItem item)
    {
        if (item.prefab == null || item.count <= 0) return;

        var offset = item.useCustomOffset ? item.offset : spawnOffset;
        int spawned = 0;

        while (spawned < item.count)
        {
            if (!TryGetRandomPointOnPlanet(offset, out Vector3 spawnPosition, out Vector3 normal)
                || IsPositionOccupied(spawnPosition)) continue;

            var align = Quaternion.FromToRotation(AxisToVector(item.alignAxis), normal);
            var randomAroundNormal = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);
            var finalRotation = randomAroundNormal * align * Quaternion.Euler(rotationAdjustment);

            Instantiate(item.prefab, spawnPosition, finalRotation, planet);
            spawned++;
        }
    }

    private bool TryGetRandomPointOnPlanet(float offset, out Vector3 spawnPosition, out Vector3 normal)
    {
        var randomDirection = Random.onUnitSphere;
        var rayOrigin = planet.position + randomDirection * (planetRadius + offset);

        if (Physics.Raycast(rayOrigin, -randomDirection, out var hit, Mathf.Infinity))
        {
            if (hit.collider.transform != planet && !hit.collider.transform.IsChildOf(planet))
            {
                spawnPosition = Vector3.zero;
                normal = Vector3.up;
                return false;
            }

            spawnPosition = hit.point + hit.normal * offset;
            normal = hit.normal;
            return true;
        }

        spawnPosition = Vector3.zero;
        normal = Vector3.up;
        return false;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, collisionCheckRadius);
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
}