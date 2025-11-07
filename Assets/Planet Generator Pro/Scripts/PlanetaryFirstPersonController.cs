using UnityEngine;
using UnityEngine.Serialization;

namespace PlanetaryFirstPersonController
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlanetaryFirstPersonController : MonoBehaviour
    {
        [Header("References")]
        public Transform planet;
        public Transform playerCamera;
        public LayerMask groundLayers = ~0;

        [Header("Movement")]
        public float moveSpeed = 6f;
        public float sprintMultiplier = 1.8f;
        public float jumpHeight = 1.6f;
        [Range(0f, 1f)] public float airControl = 0.6f;
        [Tooltip("Small downward speed used to keep the player grounded.")]
        public float groundStickSpeed = 0.5f;

        [Header("Mouse Look")]
        [Tooltip("General mouse sensitivity applied to both X and Y. Recommended range: 2 - 5.")]
        public float mouseSensitivity = 3.5f;
        [Tooltip("0 = no smoothing, >0 = smoothing speed (1/s).")]
        public float mouseSmoothing = 12f;
        public float maxLookAngle = 85f;
        public bool invertY = false;

        // Legacy serialized fields — hidden and used only for safe migration.
        [SerializeField, HideInInspector, FormerlySerializedAs("mouseSensitivityX")]
        private float legacyMouseSensitivityX;
        [SerializeField, HideInInspector, FormerlySerializedAs("mouseSensitivityY")]
        private float legacyMouseSensitivityY;
        [SerializeField, HideInInspector]
        private bool legacyMouseSensitivityMigrated = false;

        [Header("Gravity")]
        public bool useInverseSquare = false;
        public float gravityAtSurface = 9.81f;
        public float planetRadius = 5000f;
        [Tooltip("Distance used for ground checks from the player's feet.")]
        public float groundCheckDistance = 0.15f;

        [Header("Rotation / Smoothing")]
        [Tooltip("Speed for aligning body to the surface normal.")]
        public float alignSpeed = 10f;

        [Header("Spawn / Safety")]
        [Tooltip("If the player starts intersecting geometry, offset outward by this clearance.")]
        public float spawnClearance = 0.05f;
        [Tooltip("Maximum distance to search for surface during spawn resolution.")]
        public float spawnSearchDistance = 2000f;

        // internals
        Rigidbody rb;
        CapsuleCollider col;

        // input
        float inputX, inputZ;
        bool wantJump;
        bool wantSprint;

        // mouse state
        float smoothedMouseX = 0f;
        float smoothedMouseY = 0f;
        float yawAngle = 0f; // absolute yaw around desiredUp in degrees
        float pitch = 0f;    // camera pitch (local)

        // physics state
        bool isGrounded = false;
        Vector3 lastGroundNormal = Vector3.up;
        float verticalVelAlongUp = 0f;

        // cache for desired rotation (keeps Update/FixedUpdate consistent)
        Quaternion desiredRotationCache = Quaternion.identity;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();

            // Rigidbody setup
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            col.direction = 1; // Y

            // lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (planet != null)
            {
                AlignInitialRotationAndYaw();
                ResolveSpawnIfInsideGeometry();
            }

            rb.linearVelocity = Vector3.zero;
        }

        void OnValidate()
        {
            // Safe migration from legacy mouseSensitivityX/Y to single mouseSensitivity.
            // If legacy values look large (likely degrees/sec), scale them down by ~60.
            if (!legacyMouseSensitivityMigrated)
            {
                if (legacyMouseSensitivityX > 1e-6f || legacyMouseSensitivityY > 1e-6f)
                {
                    float converted = 0f;
                    int cnt = 0;
                    if (legacyMouseSensitivityX > 1e-6f)
                    {
                        converted += (legacyMouseSensitivityX > 20f) ? legacyMouseSensitivityX / 60f : legacyMouseSensitivityX;
                        cnt++;
                    }
                    if (legacyMouseSensitivityY > 1e-6f)
                    {
                        converted += (legacyMouseSensitivityY > 20f) ? legacyMouseSensitivityY / 60f : legacyMouseSensitivityY;
                        cnt++;
                    }
                    if (cnt > 0)
                    {
                        mouseSensitivity = converted / cnt;
                    }

                    legacyMouseSensitivityX = 0f;
                    legacyMouseSensitivityY = 0f;
                }

                legacyMouseSensitivityMigrated = true;
            }
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            // Movement input (raw for responsiveness)
            inputX = Input.GetAxisRaw("Horizontal");
            inputZ = Input.GetAxisRaw("Vertical");
            wantJump = wantJump || Input.GetButtonDown("Jump");
            wantSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (planet == null) return;

            // MOUSE INPUT: use GetAxisRaw and do NOT multiply by Time.deltaTime.
            float rawX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float rawY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

            // Exponential smoothing (mouseSmoothing is a speed in 1/s)
            if (mouseSmoothing > 1e-5f)
            {
                float alpha = 1f - Mathf.Exp(-mouseSmoothing * Time.deltaTime);
                smoothedMouseX = Mathf.Lerp(smoothedMouseX, rawX, alpha);
                smoothedMouseY = Mathf.Lerp(smoothedMouseY, rawY, alpha);
            }
            else
            {
                smoothedMouseX = rawX;
                smoothedMouseY = rawY;
            }

            // Accumulate yaw (degrees)
            yawAngle += smoothedMouseX;
            yawAngle = NormalizeAngleDeg(yawAngle);

            // Camera pitch with clamping for responsiveness
            pitch += smoothedMouseY;
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
            if (playerCamera) playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Compute desired rotation each frame so visuals are immediate
            {
                Vector3 toCenter = (planet.position - transform.position);
                Vector3 gravityDir = toCenter.normalized;
                Vector3 desiredUp = -gravityDir;

                Vector3 refForward = Vector3.ProjectOnPlane(Vector3.forward, desiredUp);
                if (refForward.sqrMagnitude < 1e-6f) refForward = Vector3.ProjectOnPlane(Vector3.right, desiredUp);
                refForward.Normalize();

                Vector3 desiredForward = Quaternion.AngleAxis(yawAngle, desiredUp) * refForward;
                desiredForward = Vector3.ProjectOnPlane(desiredForward, desiredUp).normalized;

                desiredRotationCache = Quaternion.LookRotation(desiredForward, desiredUp);

                // Apply visual rotation immediately to avoid perceptual lag
                transform.rotation = desiredRotationCache;
            }
        }

        void FixedUpdate()
        {
            if (planet == null) return;
            float dt = Time.fixedDeltaTime;

            // Gravity direction and desired up
            Vector3 toCenter = (planet.position - transform.position);
            float distance = toCenter.magnitude;
            Vector3 gravityDir = toCenter.normalized;
            Vector3 desiredUp = -gravityDir;

            // Ground check (spherecast)
            Vector3 centerWorld = transform.TransformPoint(col.center);
            float sphereRadius = Mathf.Max(0.01f, col.radius * 0.9f);
            float castDist = Mathf.Max(0.01f, (col.height * 0.5f - col.radius) + groundCheckDistance);

            RaycastHit hit;
            isGrounded = Physics.SphereCast(centerWorld, sphereRadius, gravityDir, out hit, castDist, groundLayers, QueryTriggerInteraction.Ignore);

            Vector3 groundNormal = lastGroundNormal;
            if (isGrounded)
            {
                groundNormal = hit.normal;
                lastGroundNormal = groundNormal;
            }

            // Build desired rotation using yaw and desiredUp
            Vector3 refForward = Vector3.ProjectOnPlane(Vector3.forward, desiredUp);
            if (refForward.sqrMagnitude < 1e-6f) refForward = Vector3.ProjectOnPlane(Vector3.right, desiredUp);
            refForward.Normalize();

            Vector3 desiredForward = Quaternion.AngleAxis(yawAngle, desiredUp) * refForward;
            desiredForward = Vector3.ProjectOnPlane(desiredForward, desiredUp).normalized;

            Quaternion desiredRotation = Quaternion.LookRotation(desiredForward, desiredUp);

            // Apply rotation to Rigidbody (keeps physics consistent with visuals)
            rb.MoveRotation(desiredRotation);

            // Movement basis
            Vector3 tangentForward = desiredForward;
            Vector3 tangentRight = Vector3.Cross(desiredUp, tangentForward).normalized;

            // Desired horizontal velocity
            Vector3 inputDir = (tangentRight * inputX + tangentForward * inputZ);
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
            float speed = moveSpeed * (wantSprint ? sprintMultiplier : 1f);
            Vector3 desiredHorVel = inputDir * speed;

            // Gravity magnitude
            float g = gravityAtSurface;
            if (useInverseSquare)
            {
                float r = Mathf.Max(distance, 1e-4f);
                g = gravityAtSurface * (planetRadius * planetRadius) / (r * r);
            }

            // Vertical handling (along desiredUp)
            if (isGrounded)
            {
                if (wantJump)
                {
                    verticalVelAlongUp = Mathf.Sqrt(2f * g * Mathf.Max(0.001f, jumpHeight));
                    isGrounded = false;
                }
                else
                {
                    if (verticalVelAlongUp < 0f) verticalVelAlongUp = -groundStickSpeed;
                    // Follow slope
                    desiredHorVel = Vector3.ProjectOnPlane(desiredHorVel, groundNormal);
                }
            }
            else
            {
                verticalVelAlongUp -= g * dt;
                desiredHorVel *= airControl;
            }

            Vector3 finalVel = desiredHorVel + desiredUp * verticalVelAlongUp;
            rb.linearVelocity = finalVel;

            wantJump = false;
        }

        // Set initial rotation and compute yaw from current transform to avoid snapping
        void AlignInitialRotationAndYaw()
        {
            Vector3 toCenter = (planet.position - transform.position);
            Vector3 gravityDir = toCenter.normalized;
            Vector3 desiredUp = -gravityDir;

            Vector3 refForward = Vector3.ProjectOnPlane(Vector3.forward, desiredUp);
            if (refForward.sqrMagnitude < 1e-6f) refForward = Vector3.ProjectOnPlane(Vector3.right, desiredUp);
            refForward.Normalize();

            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, desiredUp);
            if (projectedForward.sqrMagnitude < 1e-6f) projectedForward = refForward;
            projectedForward.Normalize();

            yawAngle = Vector3.SignedAngle(refForward, projectedForward, desiredUp);

            Vector3 desiredForward = Quaternion.AngleAxis(yawAngle, desiredUp) * refForward;
            Quaternion desiredRotation = Quaternion.LookRotation(desiredForward, desiredUp);
            transform.rotation = desiredRotation;
            rb.rotation = desiredRotation;
        }

        // If starting inside geometry, try to move player out along nearest surface
        void ResolveSpawnIfInsideGeometry()
        {
            Vector3 centerWorld = transform.TransformPoint(col.center);
            Vector3 upWorld = transform.up;
            float half = Mathf.Max(0.01f, col.height * 0.5f - col.radius);
            Vector3 pTop = centerWorld + upWorld * half;
            Vector3 pBot = centerWorld - upWorld * half;

            Collider[] overlaps = Physics.OverlapCapsule(pBot, pTop, col.radius, groundLayers, QueryTriggerInteraction.Ignore);
            if (overlaps != null && overlaps.Length > 0)
            {
                Vector3 toCenter = (planet.position - transform.position);
                Vector3 gravityDir = toCenter.normalized;
                Vector3 desiredUp = -gravityDir;

                RaycastHit hitUp;
                if (Physics.Raycast(transform.position, desiredUp, out hitUp, spawnSearchDistance, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    Vector3 wantedPos = hitUp.point + desiredUp * (col.height * 0.5f + spawnClearance);
                    transform.position = wantedPos;
                    rb.position = wantedPos;
                    rb.linearVelocity = Vector3.zero;
                    return;
                }

                RaycastHit hitDown;
                if (Physics.Raycast(transform.position, -desiredUp, out hitDown, spawnSearchDistance, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    Vector3 wantedPos = hitDown.point + desiredUp * (col.height * 0.5f + spawnClearance);
                    transform.position = wantedPos;
                    rb.position = wantedPos;
                    rb.linearVelocity = Vector3.zero;
                    return;
                }

                for (int i = 0; i < 20; i++)
                {
                    transform.position += desiredUp * (spawnClearance + 0.02f);
                    centerWorld = transform.TransformPoint(col.center);
                    pTop = centerWorld + upWorld * half;
                    pBot = centerWorld - upWorld * half;
                    overlaps = Physics.OverlapCapsule(pBot, pTop, col.radius, groundLayers, QueryTriggerInteraction.Ignore);
                    if (overlaps == null || overlaps.Length == 0)
                    {
                        rb.position = transform.position;
                        rb.linearVelocity = Vector3.zero;
                        return;
                    }
                }
            }
        }

        static float NormalizeAngleDeg(float a)
        {
            a %= 360f;
            if (a > 180f) a -= 360f;
            if (a < -180f) a += 360f;
            return a;
        }

        void OnDrawGizmosSelected()
        {
            if (planet == null || col == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);

            Vector3 toCenter = (planet.position - transform.position);
            Vector3 gravityDir = toCenter.normalized;
            Vector3 desiredUp = -gravityDir;

            Vector3 fwdProj = Vector3.ProjectOnPlane(transform.forward, desiredUp).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + fwdProj * 1.5f);

            Vector3 tangentRight = Vector3.Cross(desiredUp, fwdProj).normalized;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + tangentRight * 1.5f);

            Vector3 centerWorld = transform.TransformPoint(col.center);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(centerWorld, col.radius * 0.9f);
        }
    }
}
