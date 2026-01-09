using System.Collections;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour
{
    [SerializeField] private string planetTag = "Planet";
    [SerializeField] private GameObject[] resourcePrefabs;
    [SerializeField] private int numberOfResources = 100;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float collisionCheckRadius = 1.0f;

    private Transform planet;
    private float planetRadius;

    void Start() => StartCoroutine(WaitForPlanetGeneration());

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(1.2f);

        FindPlanet();
        if (planet == null) yield break;

        SpawnResources();
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

    private void SpawnResources()
    {
        var spawnedResources = 0;

        while (spawnedResources < numberOfResources)
        {
            if (TryGetRandomPointOnPlanet(out Vector3 spawnPosition, out Quaternion spawnRotation)
                && !IsPositionOccupied(spawnPosition))
            {
                Instantiate(GetRandomResourcePrefab(), spawnPosition, spawnRotation, planet);
                spawnedResources++;
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
            if (collider.CompareTag("Resource")) return true;
        }

        return false;
    }

    private GameObject GetRandomResourcePrefab() => resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
}