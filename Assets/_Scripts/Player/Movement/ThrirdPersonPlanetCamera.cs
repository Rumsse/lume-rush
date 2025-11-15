using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThrirdPersonPlanetCamera : MonoBehaviour
{
    #region Inspector

    [SerializeField] Transform target;
    [SerializeField] float distance = 5f;
    [SerializeField] float heightOffset = 1.6f;
    [SerializeField] float pitchAngle = 35f;
    [SerializeField] float smoothTime = 0.08f;
    [SerializeField] float rotationSpeed = 8f;
    [SerializeField] LayerMask collisionMask = ~0;
    [SerializeField] float collisionRadius = 0.25f;
    [SerializeField] float collisionOffset = 0.2f;

    #endregion

    #region State

    Vector3 velocity;
    Vector3 lastForward = Vector3.forward;

    #endregion

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 pivot = target.position + target.up * heightOffset;
        Vector3 tangentF = GetTangentForward();
        if (tangentF.sqrMagnitude > 0.0001f) lastForward = Vector3.Slerp(lastForward, tangentF, Time.deltaTime * rotationSpeed);
        Vector3 dir = Quaternion.AngleAxis(pitchAngle, target.right) * (-lastForward);
        Vector3 desiredPos = pivot + dir.normalized * distance;
        desiredPos = ResolveCollision(pivot, desiredPos);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
        Vector3 lookDir = (pivot - transform.position).normalized;
        if (lookDir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(lookDir, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    Vector3 GetTangentForward()
    {
        Vector3 f = target.forward;
        Vector3 n = target.up;
        Vector3 ortho = Vector3.ProjectOnPlane(f, n);
        return ortho.sqrMagnitude < 0.0001f ? Vector3.zero : ortho.normalized;
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
}