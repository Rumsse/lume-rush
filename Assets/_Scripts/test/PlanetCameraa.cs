using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlanetCameraa : MonoBehaviour
{
    #region Inspector

    [SerializeField] Transform target;
    [SerializeField] float distance = 5f;
    [SerializeField] float heightOffset = 1.6f;
    [SerializeField] float pitchAngle = 35f;

    [Header("Jump / vertical lag")]
    [SerializeField] float verticalLagAmount = 0.5f;

    [Header("Collision")]
    [SerializeField] LayerMask collisionMask = ~0;
    [SerializeField] float collisionRadius = 0.25f;
    [SerializeField] float collisionOffset = 0.2f;

    [Header("Tuning")]
    [SerializeField] float minVelocityForForward = 0.3f;

    #endregion

    #region State

    Vector3 currentForward = Vector3.forward;
    Rigidbody targetRigidbody;

    #endregion

    #region MonoBehaviour lifecycle

    void Start()
    {
        if (target == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) target = found.transform;
        }

        if (target != null) targetRigidbody = target.GetComponent<Rigidbody>();
        var initial = GetProjectedForwardFromTarget();
        if (initial.sqrMagnitude > 0.0001f) currentForward = initial.normalized;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 rawPivot = target.position + target.up * heightOffset;

        Vector3 bestForward = GetBestTangentForward();
        if (bestForward.sqrMagnitude > 0.0001f) currentForward = bestForward.normalized;

        float upVelAlongSurface = targetRigidbody != null ? Vector3.Dot(targetRigidbody.linearVelocity, target.up) : 0f;
        Vector3 pivotWithVerticalLag = rawPivot + target.up * (-upVelAlongSurface * verticalLagAmount);

        Vector3 pitchedDirection = Quaternion.AngleAxis(pitchAngle, target.right) * (-currentForward.normalized);
        Vector3 desiredCameraPos = pivotWithVerticalLag + pitchedDirection.normalized * distance;
        desiredCameraPos = ResolveCollision(pivotWithVerticalLag, desiredCameraPos);

        transform.position = desiredCameraPos;

        Vector3 lookDir = (pivotWithVerticalLag - transform.position).normalized;
        if (lookDir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(lookDir, target.up);
    }

    #endregion

    #region Helpers

    Vector3 GetProjectedForwardFromTarget()
    {
        if (target == null) return Vector3.zero;
        Vector3 proj = Vector3.ProjectOnPlane(target.forward, target.up);
        return proj.sqrMagnitude < 0.0001f ? Vector3.zero : proj.normalized;
    }

    Vector3 GetBestTangentForward()
    {
        Vector3 surfaceUp = target.up;

        if (targetRigidbody != null)
        {
            Vector3 vel = Vector3.ProjectOnPlane(targetRigidbody.linearVelocity, surfaceUp);
            if (vel.magnitude >= minVelocityForForward) return vel.normalized;
        }

        var projectedForward = GetProjectedForwardFromTarget();
        if (projectedForward.sqrMagnitude > 0.0001f) return projectedForward;

        Vector3 camToPivot = transform.position - (target.position + surfaceUp * heightOffset);
        Vector3 camTangent = Vector3.ProjectOnPlane(camToPivot, surfaceUp);
        if (camTangent.sqrMagnitude > 0.0001f) return (-camTangent).normalized;

        return currentForward;
    }

    Vector3 ResolveCollision(Vector3 origin, Vector3 desired)
    {
        Vector3 dir = desired - origin;
        float dist = dir.magnitude;
        if (dist < 0.001f) return desired;
        if (Physics.SphereCast(origin, collisionRadius, dir.normalized, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            return hit.point - dir.normalized * (collisionOffset + collisionRadius);
        return desired;
    }

    #endregion
}