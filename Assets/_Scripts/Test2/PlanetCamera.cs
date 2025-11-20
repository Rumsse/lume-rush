using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlanetCamera : MonoBehaviour
{
    #region Inspector

    [SerializeField] Transform target;
    [SerializeField] float baseDistance = 4f;
    [SerializeField] float minDistanceWhenRunning = 2.6f;
    [SerializeField] float maxDistanceWhenJumping = 6f;
    [SerializeField] float distanceSmoothTime = 0.12f;

    [SerializeField] float baseHeight = 1.2f;
    [SerializeField] float maxHeightWhenRunning = 2.2f;
    [SerializeField] float jumpHeightOffset = 1f;

    [SerializeField] float rotationSmooth = 10f;
    [SerializeField] float surfaceNormalSmooth = 12f;
    [SerializeField] float forwardSmooth = 12f;

    [Header("Focus on planet (dynamic)")]
    [SerializeField][Range(0f, 1f)] float baseFocusOnPlanetWeight = 0.35f;
    [SerializeField][Range(0f, 1f)] float maxFocusOnPlanetWeight = 0.6f;
    [SerializeField][Range(0f, 1f)] float jumpFocusWeight = 0.15f;
    [SerializeField] float focusAdjustSpeed = 3f;

    [Header("Forward detection")]
    [SerializeField] float speedForFullEffect = 4f;
    [SerializeField] float minForwardDot = 0.0001f;

    [Header("Jump detection")]
    [SerializeField] float jumpNormalVelocityThreshold = 0.6f;
    [SerializeField] float groundDistanceTolerance = 0.12f;

    #endregion

    #region State

    Vector3 posVelocity;
    float distanceVelocity;
    float currentFocusWeight;
    float currentDistance;
    Rigidbody targetRb;
    CapsuleCollider targetCapsule;
    float targetHalfHeight;

    Vector3 smoothSurfaceNormal = Vector3.up;
    Vector3 smoothForward = Vector3.forward;

    #endregion

    void Awake()
    {
        if (target == null)
        {
            var playerPMove = FindFirstObjectByType<PlayerPlanetMovement>();
            if (playerPMove != null) target = playerPMove.transform;
        }

        currentFocusWeight = baseFocusOnPlanetWeight;
        currentDistance = baseDistance;
        if (target != null)
        {
            targetRb = target.GetComponentInParent<Rigidbody>();
            targetCapsule = target.GetComponentInParent<CapsuleCollider>();
            targetHalfHeight = targetCapsule != null ? Mathf.Max(0.01f, targetCapsule.height * 0.5f) : 0.5f;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        var planet = GetNearestPlanet();

        Vector3 targetPos = targetRb != null ? targetRb.position : target.position;
        Quaternion targetRot = targetRb != null ? targetRb.rotation : target.rotation;
        
        Vector3 rawSurfaceNormal = planet != null ? (targetPos - planet.Center).normalized : Vector3.up;
        smoothSurfaceNormal = Vector3.Slerp(smoothSurfaceNormal, rawSurfaceNormal, Mathf.Clamp01(Time.deltaTime * surfaceNormalSmooth));
        
        Vector3 rawForward = (targetRot * Vector3.forward);
        Vector3 rawProjectedForward = Vector3.ProjectOnPlane(rawForward, smoothSurfaceNormal);
        if (rawProjectedForward.sqrMagnitude <= minForwardDot)
            rawProjectedForward = Vector3.ProjectOnPlane(targetRot * Vector3.right, smoothSurfaceNormal);
        rawProjectedForward.Normalize();
        smoothForward = Vector3.Slerp(smoothForward, rawProjectedForward, Mathf.Clamp01(Time.deltaTime * forwardSmooth));

        Vector3 tangentialVel = targetRb != null ? Vector3.ProjectOnPlane(targetRb.linearVelocity, smoothSurfaceNormal) : Vector3.zero;
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(tangentialVel, smoothForward));
        float forwardFactor = Mathf.Clamp01(forwardSpeed / Mathf.Max(0.0001f, speedForFullEffect));

        bool isJumping = false;
        if (planet != null)
        {
            float distToCenter = Vector3.Distance(targetPos, planet.Center);
            if (distToCenter > planet.SurfaceRadius + targetHalfHeight + groundDistanceTolerance) isJumping = true;
        }
        if (!isJumping && targetRb != null)
        {
            float normalVel = Vector3.Dot(targetRb.linearVelocity, smoothSurfaceNormal);
            if (normalVel > jumpNormalVelocityThreshold) isJumping = true;
        }

        float targetFocusWeight = isJumping ? jumpFocusWeight : Mathf.Lerp(baseFocusOnPlanetWeight, maxFocusOnPlanetWeight, forwardFactor);
        currentFocusWeight = Mathf.MoveTowards(currentFocusWeight, targetFocusWeight, focusAdjustSpeed * Time.deltaTime);

        float targetDistance = isJumping ? maxDistanceWhenJumping : Mathf.Lerp(baseDistance, minDistanceWhenRunning, forwardFactor);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, distanceSmoothTime);
        float currentHeight = isJumping ? baseHeight + jumpHeightOffset : Mathf.Lerp(baseHeight, maxHeightWhenRunning, forwardFactor);

        Vector3 desiredPos = targetPos - smoothForward * currentDistance + smoothSurfaceNormal * currentHeight;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVelocity, distanceSmoothTime);

        Vector3 lookAtPoint = planet != null ? Vector3.Lerp(targetPos, planet.Center, currentFocusWeight) : targetPos;
        Quaternion desiredRot = Quaternion.LookRotation((lookAtPoint - transform.position).normalized, smoothSurfaceNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Mathf.Clamp01(Time.deltaTime * rotationSmooth));
    }

    Planet GetNearestPlanet()
    {
        var list = Planet.All;
        if (list == null || list.Count == 0) return null;
        Vector3 refPos = targetRb != null ? targetRb.position : (target != null ? target.position : Vector3.zero);
        var nearest = list.OrderBy(p => (p.Center - refPos).sqrMagnitude).FirstOrDefault();
        if (nearest == null) return null;
        return Vector3.Distance(refPos, nearest.Center) <= nearest.GravityRadius ? nearest : null;
    }
}