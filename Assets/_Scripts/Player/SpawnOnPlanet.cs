using System.Collections;
using UnityEngine;

public class SpawnOnPlanet : MonoBehaviour
{
    [SerializeField] private string planetTag = "Planet";
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float collisionCheckRadius = 1.0f;
    [SerializeField] private float shipSpawnDistance = 5.0f;
    [SerializeField] private float shipHeightAboveSurface = 1.0f;
    [SerializeField] private int maxSearchAttempts = 50;

    private Transform planet;
    private float planetRadius;

    void Start() => StartCoroutine(WaitForPlanetGeneration());

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(0.15f);

        FindPlanet();
        if (planet == null)
        {
            Debug.LogError("No planet found with the tag: " + planetTag);
            yield break;
        }

        SpawnPlayerAndShip();
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

    private void SpawnPlayerAndShip()
    {
        if (TryGetRandomPointOnPlanet(out Vector3 playerPosition, out Quaternion playerRotation))
        {
            Instantiate(playerPrefab, playerPosition, playerRotation);

            FindSafePositionForShip(playerPosition, playerRotation);
        }
        else
        {
            Debug.LogError("Failed to find a valid spawn point for the player.");
        }
    }

    private void FindSafePositionForShip(Vector3 playerPosition, Quaternion playerRotation)
    {
        int attempts = 0;

        while (attempts < maxSearchAttempts)
        {
            Vector3 shipPosition = playerPosition + playerRotation * (Vector3.forward * shipSpawnDistance);

            Vector3 shipRaycastOrigin = shipPosition + playerRotation * Vector3.up * spawnOffset;
            if (Physics.Raycast(shipRaycastOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                shipPosition = hit.point + hit.normal * (spawnOffset + shipHeightAboveSurface);
                if (!IsPositionOccupied(shipPosition))
                {
                    Quaternion shipRotation = Quaternion.LookRotation(Vector3.Cross(playerRotation * Vector3.right, hit.normal), hit.normal);
                    Instantiate(shipPrefab, shipPosition, shipRotation);
                    return;
                }
            }

            playerRotation = Quaternion.Euler(0, 10f, 0) * playerRotation; // Obrót wokó³ osi Y
            attempts++;
        }

        ForceSpawnShip(playerPosition, playerRotation);
    }

    private void ForceSpawnShip(Vector3 playerPosition, Quaternion playerRotation)
    {
        Vector3 forcedPosition = playerPosition + playerRotation * (Vector3.forward * shipSpawnDistance);
        forcedPosition += Vector3.up * (spawnOffset + shipHeightAboveSurface * 5); // Wymuszone podniesienie nad powierzchni¹

        Quaternion forcedRotation = Quaternion.identity;

        Debug.LogWarning("Forcing ship spawn at higher altitude to avoid collision.");
        Instantiate(shipPrefab, forcedPosition, forcedRotation);
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
            if (collider.CompareTag("Resource") || collider.CompareTag("Player") || collider.CompareTag("SpaceShip")) return true;
        }

        return false;
    }
}