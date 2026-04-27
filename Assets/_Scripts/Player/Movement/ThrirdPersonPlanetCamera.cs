using UnityEngine;
using System.Collections;

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

    [Header("Jump response")]
    [SerializeField] float jumpDistanceMultiplier = 0.6f;
    [SerializeField] float jumpExtraHeight = 0.8f;
    [SerializeField] float jumpPitchAdd = 12f;
    [SerializeField] float jumpDetectThreshold = 0.6f;
    [SerializeField] float jumpTransitionSpeed = 6f;

    [Header("Jump Look Target")]
    [SerializeField] Transform jumpLookTarget;
    [SerializeField] bool useJumpLookTarget = true;

    [Header("Look smoothing")]
    [SerializeField][Range(0.01f, 1f)] float lookSmoothTime = 0.18f;

    #endregion

    #region State

    Vector3 velocity;
    Vector3 lastForward = Vector3.forward;
    Rigidbody targetRb;
    float currentDistance;
    float currentHeight;
    float currentPitch;
    float distanceVel;
    float heightVel;
    Vector3 currentLookPoint;
    Vector3 lookVelocity;

    #endregion

    void Start() => StartCoroutine(WaitForPlayerSpawn());

    IEnumerator WaitForPlayerSpawn()
    {
        yield return new WaitForSeconds(0.3f);

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (target == null)
                Debug.LogError("Player object with tag 'Player' not found in the scene!");
        }

        targetRb = target.GetComponentInParent<Rigidbody>();
        currentDistance = distance;
        currentHeight = heightOffset;
        currentPitch = pitchAngle;
        currentLookPoint = target.position + target.up * heightOffset;
        lookVelocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pivot = target.position + target.up * heightOffset;

        Vector3 tangentF = GetTangentForward();
        if (tangentF.sqrMagnitude > 0.0001f)
            lastForward = Vector3.Slerp(lastForward, tangentF, Time.deltaTime * rotationSpeed);

        bool isJumping = false;
        if (targetRb != null)
        {
            float normalVel = Vector3.Dot(targetRb.linearVelocity, target.up);
            if (normalVel > jumpDetectThreshold) isJumping = true;
        }

        float targetDist = isJumping ? distance * jumpDistanceMultiplier : distance;
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref distanceVel, smoothTime);

        float targetH = isJumping ? heightOffset + jumpExtraHeight : heightOffset;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetH, ref heightVel, smoothTime);

        float targetPitch = isJumping ? pitchAngle + jumpPitchAdd : pitchAngle;
        currentPitch = Mathf.MoveTowards(currentPitch, targetPitch, jumpTransitionSpeed * Time.deltaTime);

        Vector3 dir = Quaternion.AngleAxis(currentPitch, target.right) * (-lastForward);
        Vector3 desiredPos = pivot + dir.normalized * currentDistance;
        desiredPos = ResolveCollision(pivot, desiredPos);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);

        Vector3 desiredLookPoint = pivot;
        if (isJumping && useJumpLookTarget && jumpLookTarget != null)
            desiredLookPoint = jumpLookTarget.position;

        currentLookPoint = Vector3.SmoothDamp(currentLookPoint, desiredLookPoint, ref lookVelocity, lookSmoothTime);

        Vector3 lookDir = (currentLookPoint - transform.position).normalized;
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

    public void SetJumpLookTarget(Transform t) => jumpLookTarget = t;
}