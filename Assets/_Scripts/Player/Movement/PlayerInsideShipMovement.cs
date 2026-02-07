using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInsideShipMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float movementSmoothing = 0.15f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpCooldown = 0.3f;

    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    #endregion

    #region Private Fields

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private bool isRunning;
    private bool isGrounded;
    private bool wasGrounded;
    private float jumpTimer;
    private float currentSpeed;

    private bool hasSpeedParam;
    private bool hasIsGroundedParam;
    private bool hasIsRunningParam;
    private bool hasJumpParam;
    private bool hasLandParam;

    #endregion

    #region Properties

    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    #endregion

    #region Unity Lifecycle

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Start()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        CheckAnimatorParameters();
    }

    private void Update()
    {
        HandleInput();
        CheckGround();
        CalculateMovementDirection();
        UpdateAnimations();
        UpdateJumpTimer();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    #endregion

    #region Animator Setup

    private void CheckAnimatorParameters()
    {
        if (animator == null)
            return;

        hasSpeedParam = HasParameter("Speed", AnimatorControllerParameterType.Float);
        hasIsGroundedParam = HasParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        hasIsRunningParam = HasParameter("IsRunning", AnimatorControllerParameterType.Bool);
        hasJumpParam = HasParameter("Jump", AnimatorControllerParameterType.Trigger);
        hasLandParam = HasParameter("Land", AnimatorControllerParameterType.Trigger);
    }

    private bool HasParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == type)
                return true;
        }
        return false;
    }

    #endregion

    #region Input

    private void HandleInput()
    {
        moveInput.x = Input.GetAxisRaw(horizontalAxis);
        moveInput.y = Input.GetAxisRaw(verticalAxis);
        isRunning = Input.GetKey(runKey);

        if (Input.GetKeyDown(jumpKey) && isGrounded && jumpTimer <= 0f)
            Jump();
    }

    #endregion

    #region Ground Check

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        Vector3 origin = transform.position + groundCheckOffset;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && !wasGrounded && animator != null && hasLandParam)
            animator.SetTrigger("Land");
    }

    #endregion

    #region Movement

    private void CalculateMovementDirection()
    {
        if (!IsMoving)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Transform cam = CameraController.Instance.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
    }

    private void ApplyMovement()
    {
        if (!isGrounded)
            return;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 targetVelocity = moveDirection * targetSpeed;

        Vector3 velocity = Vector3.SmoothDamp(
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z),
            targetVelocity,
            ref currentVelocity,
            movementSmoothing
        );

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void ApplyRotation()
    {
        if (!IsMoving || moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    #endregion

    #region Jump

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpTimer = jumpCooldown;

        if (animator != null && hasJumpParam)
            animator.SetTrigger("Jump");
    }

    private void UpdateJumpTimer()
    {
        if (jumpTimer > 0f)
            jumpTimer -= Time.deltaTime;
    }

    #endregion

    #region Animations

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        currentSpeed = horizontalVelocity.magnitude;

        if (hasSpeedParam)
            animator.SetFloat("Speed", currentSpeed);

        if (hasIsGroundedParam)
            animator.SetBool("IsGrounded", isGrounded);

        if (hasIsRunningParam)
            animator.SetBool("IsRunning", isRunning && IsMoving);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.1f);
    }

    #endregion
}