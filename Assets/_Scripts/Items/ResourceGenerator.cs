using System.Collections;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour
{
    [SerializeField] private string planetTag = "Planet";

    [Header("Resources")]
    [SerializeField] private GameObject[] resourcePrefabs;
    [SerializeField] private int[] resourceCounts;

    [Header("Props")]
    [SerializeField] private GameObject[] propPrefabs;
    [SerializeField] private int[] propCounts;

    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float collisionCheckRadius = 1.0f;
    [SerializeField] private Vector3 rotationAdjustment = Vector3.zero;

    private Transform planet;
    private float planetRadius;

    void Start() => StartCoroutine(WaitForPlanetGeneration());

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(0.1f);

        FindPlanet();
        if (planet == null) yield break;

        SpawnPrefabs(resourcePrefabs, resourceCounts);
        SpawnPrefabs(propPrefabs, propCounts);
    }

    private void FindPlanet()
    {
        var planetObject = GameObject.FindGameObjectWithTag(planetTag);
        if (planetObject == null) return;

        planet = planetObject.transform;
        planetRadius = CalculatePlanetRadius(planet);
    }

    private float CalculatePlanetRadius(Transform planet)
    {
        var renderer = planet.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return 0f;

        var bounds = renderer.bounds;
        return Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
    }

    private void SpawnPrefabs(GameObject[] prefabs, int[] counts)
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            int toSpawn = counts.Length > i ? counts[i] : 0;
            SpawnPrefabInstances(prefabs[i], toSpawn);
        }
    }

    private void SpawnPrefabInstances(GameObject prefab, int count)
    {
        int spawned = 0;

        while (spawned < count)
        {
            if (TryGetRandomPointOnPlanet(out Vector3 spawnPosition, out Quaternion spawnRotation)
                && !IsPositionOccupied(spawnPosition))
            {
                Quaternion adjustedRotation = spawnRotation * Quaternion.Euler(rotationAdjustment);

                Instantiate(prefab, spawnPosition, adjustedRotation, planet);
                spawned++;
            }
        }
    }

    private bool TryGetRandomPointOnPlanet(out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        var randomDirection = Random.onUnitSphere;
        var rayOrigin = planet.position + randomDirection * (planetRadius + spawnOffset);

        if (Physics.Raycast(rayOrigin, -randomDirection, out var hit, Mathf.Infinity))
        {
            if (hit.collider.transform == planet || hit.collider.transform.IsChildOf(planet))
            {
                spawnPosition = hit.point + hit.normal * spawnOffset;
                spawnRotation = Quaternion.LookRotation(hit.normal);
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        spawnRotation = Quaternion.identity;
        return false;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        var colliders = Physics.OverlapSphere(position, collisionCheckRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Resource") || collider.CompareTag("Prop")) return true;
        }

        return false;
    }
}