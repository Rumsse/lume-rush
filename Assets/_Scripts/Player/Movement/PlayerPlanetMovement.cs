using System.Collections;
using UnityEngine;

public class PlayerPlanetMovement : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Animator _animator;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;

    [SerializeField] private float _baseSpeed = 15f;
    [SerializeField] private float _baseTurnSpeed = 1500f;
    [SerializeField] private Vector3 _groundCheckBoxSize = new Vector3(1.2f, 0.2f, 1.8f);

    [Range(1f, 350f)]
    [SerializeField] private float _baseJumpForce = 100f;

    [Header("Dash (Sprint)")]
    [SerializeField] private float _baseDashForce = 40f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1.5f;
    [SerializeField] private GameObject _dashParticleDust;

    [Header("Air control")]
    [SerializeField][Range(0f, 1f)] private float _airMoveMultiplier = 0.6f;
    [SerializeField][Range(0f, 1f)] private float _airTurnMultiplier = 0.6f;

    [Header("Gravity Scaling")]
    [SerializeField] private float _referenceGravity = 800f;
    [SerializeField] private AnimationCurve _speedGravityScale = AnimationCurve.Linear(0.5f, 1.3f, 2f, 0.7f);
    [SerializeField] private AnimationCurve _jumpGravityScale = AnimationCurve.Linear(0.5f, 0.85f, 2f, 1.15f);

    #endregion

    #region State

    private GravityBody _gravityBody;
    private Rigidbody _rigidbody;
    private float _horizontal;
    private float _vertical;
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;
    private float _currentSpeed;
    private float _currentTurnSpeed;
    private float _currentJumpForce;
    private float _currentDashForce;

    #endregion

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _baseJumpForce = Mathf.Clamp(_baseJumpForce, 0f, 500f);
        UpdateMovementParameters();
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        _isGrounded = _groundCheck != null && Physics.CheckBox(_groundCheck.position, _groundCheckBoxSize * 0.5f, _groundCheck.rotation, _groundMask);

        UpdateMovementParameters();

        if (Input.GetKey(KeyCode.Space) && _isGrounded)
            Jump();

        if (Input.GetKey(KeyCode.LeftShift) && !_isDashing && _dashTimer <= 0f)
            StartCoroutine(PerformDash());

        UpdateAnimatorParameters();
        UpdateDashCooldown();
    }

    void FixedUpdate()
    {
        if (_isDashing) return;

        Vector3 gravityDir = _gravityBody != null ? _gravityBody.GravityDirection : Vector3.zero;
        Vector3 gravityUp = gravityDir != Vector3.zero ? -gravityDir.normalized : transform.up;

        bool moveInput = Mathf.Abs(_vertical) > 0.1f;
        bool turnInput = Mathf.Abs(_horizontal) > 0.1f;
        float moveMultiplier = _isGrounded ? 1f : _airMoveMultiplier;
        float turnMultiplier = _isGrounded ? 1f : _airTurnMultiplier;

        if (moveInput)
        {
            Vector3 desired = transform.forward * Mathf.Sign(_vertical);
            desired = Vector3.ProjectOnPlane(desired, gravityUp).normalized;
            Vector3 targetVel = desired * (_currentSpeed * moveMultiplier);
            Vector3 currentHorizVel = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, gravityUp);
            Vector3 velChange = targetVel - currentHorizVel;
            velChange = Vector3.ProjectOnPlane(velChange, gravityUp);
            _rigidbody.AddForce(velChange, ForceMode.VelocityChange);
        }

        if (turnInput)
        {
            float angle = _horizontal * (_currentTurnSpeed * Time.fixedDeltaTime) * turnMultiplier;
            Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, gravityUp);
            if (currentForward.sqrMagnitude <= 0.0001f)
                currentForward = Vector3.ProjectOnPlane(transform.right, gravityUp).normalized;

            Quaternion yaw = Quaternion.AngleAxis(angle, gravityUp);
            Vector3 newForward = (yaw * currentForward).normalized;
            _rigidbody.MoveRotation(Quaternion.LookRotation(newForward, gravityUp));
        }

        if (!moveInput && _isGrounded)
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, gravityUp);
            _rigidbody.AddForce(-horizontalVelocity * 5f, ForceMode.Acceleration);
        }
    }

    #region Movement Parameters

    void UpdateMovementParameters()
    {
        if (_gravityBody == null || _gravityBody.CurrentGravityArea == null)
        {
            _currentSpeed = _baseSpeed;
            _currentTurnSpeed = _baseTurnSpeed;
            _currentJumpForce = _baseJumpForce;
            _currentDashForce = _baseDashForce;
            return;
        }

        float currentGravity = _gravityBody.CurrentGravityArea.GravityForce;
        float gravityRatio = currentGravity / _referenceGravity;

        float speedScale = _speedGravityScale.Evaluate(gravityRatio);
        float jumpScale = _jumpGravityScale.Evaluate(gravityRatio);

        _currentSpeed = _baseSpeed * speedScale;
        _currentTurnSpeed = _baseTurnSpeed * speedScale;
        _currentJumpForce = _baseJumpForce * jumpScale;
        _currentDashForce = _baseDashForce * speedScale;
    }

    #endregion

    #region Jump & Dash

    void Jump() => _rigidbody.AddForce(-_gravityBody.GravityDirection * _currentJumpForce, ForceMode.Impulse);

    IEnumerator PerformDash()
    {
        _isDashing = true;

        if (_dashParticleDust != null)
            _dashParticleDust.SetActive(true);

        Vector3 dashDirection = transform.forward.normalized;
        _rigidbody.AddForce(dashDirection * _currentDashForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(_dashDuration);

        if (_dashParticleDust != null)
            _dashParticleDust.SetActive(false);

        _isDashing = false;
        _dashTimer = _dashCooldown;
    }

    void UpdateDashCooldown()
    {
        if (_dashTimer > 0f)
            _dashTimer -= Time.deltaTime;
    }

    #endregion

    #region Animation

    void UpdateAnimatorParameters()
    {
        if (_gravityBody == null) return;

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, -_gravityBody.GravityDirection);
        float speed = horizontalVelocity.magnitude;

        _animator.SetFloat("Speed", speed);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(_groundCheck.position, _groundCheck.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _groundCheckBoxSize);
        Gizmos.matrix = old;
    }

    #endregion
}