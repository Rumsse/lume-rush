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

    #region Lifecycle


    void Start() => StartCoroutine(WaitForPlanetGeneration());

    IEnumerator WaitForPlanetGeneration()
    {
        yield return new WaitForSeconds(0.2f);

        FindPlanet();
        if (planet == null)
        {
            Debug.LogError("No planet found with the tag: " + planetTag);
            yield break;
        }

        SpawnPlayerAndShip();
    }

    #endregion

    #region Planet


    private void FindPlanet()
    {
        var planetObject = GameObject.FindGameObjectWithTag(planetTag);
        if (planetObject == null) return;

        planet = planetObject.transform;
        planetRadius = CalculatePlanetRadius(planet);
    }

    private float CalculatePlanetRadius(Transform p)
    {
        var renderer = p.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return 0f;

        var bounds = renderer.bounds;
        return Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
    }

    #endregion

    #region Spawning


    private void SpawnPlayerAndShip()
    {
        if (!TryGetRandomPointOnPlanet(out var playerPosition, out var playerRotation))
        {
            Debug.LogError("Failed to find a valid spawn point for the player.");
            return;
        }

        Instantiate(playerPrefab, playerPosition, playerRotation);
        FindSafePositionForShip(playerPosition, playerRotation);
    }

    private void FindSafePositionForShip(Vector3 playerPosition, Quaternion playerRotation)
    {
        var normal = (playerPosition - planet.position).normalized;
        var forward = Vector3.ProjectOnPlane(playerRotation * Vector3.forward, normal).normalized;
        if (forward.sqrMagnitude < 1e-6f) forward = Vector3.Cross(normal, playerRotation * Vector3.right).normalized;

        var angleStep = 360f / Mathf.Max(1, maxSearchAttempts);
        var angle = 0f;

        for (int i = 0; i < maxSearchAttempts; i++)
        {
            var dir = Quaternion.AngleAxis(angle, normal) * forward;
            if (!TryRaycastToPlanet(normal, dir * shipSpawnDistance, out var hit))
            {
                angle += angleStep;
                continue;
            }

            var pos = hit.point + hit.normal * (spawnOffset + shipHeightAboveSurface);
            if (IsPositionOccupied(pos))
            {
                angle += angleStep;
                continue;
            }

            var fwdOnSurface = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
            var rot = fwdOnSurface.sqrMagnitude < 1e-6f
                ? Quaternion.LookRotation(Vector3.Cross(playerRotation * Vector3.right, hit.normal), hit.normal)
                : Quaternion.LookRotation(fwdOnSurface, hit.normal);

            Instantiate(shipPrefab, pos, rot);
            return;
        }

        ForceSpawnShip(playerPosition, playerRotation);
    }

    private void ForceSpawnShip(Vector3 playerPosition, Quaternion playerRotation)
    {
        var normal = (playerPosition - planet.position).normalized;
        var forward = Vector3.ProjectOnPlane(playerRotation * Vector3.forward, normal).normalized;
        if (forward.sqrMagnitude < 1e-6f) forward = Vector3.Cross(normal, playerRotation * Vector3.right).normalized;

        var origin = planet.position + normal * (planetRadius + spawnOffset + shipHeightAboveSurface * 6f) + forward * shipSpawnDistance;
        var pos = origin;
        var rot = Quaternion.LookRotation(forward, normal);

        Instantiate(shipPrefab, pos, rot);
    }

    #endregion

    #region Helpers


    private bool TryRaycastToPlanet(Vector3 normal, Vector3 tangentOffset, out RaycastHit hit)
    {
        var origin = planet.position + normal * (planetRadius + spawnOffset + shipHeightAboveSurface + 2f) + tangentOffset;
        var maxDist = planetRadius * 3f;

        if (Physics.Raycast(origin, -normal, out hit, maxDist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            if (hit.collider.transform == planet || hit.collider.transform.IsChildOf(planet)) return true;

        hit = default;
        return false;
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
        foreach (var c in colliders)
            if (c.CompareTag("Resource") || c.CompareTag("Player") || c.CompareTag("SpaceShip")) return true;

        return false;
    }

    #endregion
}