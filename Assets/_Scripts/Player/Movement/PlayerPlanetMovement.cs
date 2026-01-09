using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanetMovement : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Animator _animator;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;

    [SerializeField] public float _speed = 15f;
    [SerializeField] public float _turnSpeed = 1500f;
    [SerializeField] private Vector3 _groundCheckBoxSize = new Vector3(1.2f, 0.2f, 1.8f);

    [Range(50f, 350f)]
    [SerializeField] private float _jumpForce = 100f;

    [Header("Dash (Sprint)")]
    [SerializeField] private float _dashForce = 40f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1.5f;

    [Header("Air control")]
    [SerializeField][Range(0f, 1f)] private float _airMoveMultiplier = 0.6f;
    [SerializeField][Range(0f, 1f)] private float _airTurnMultiplier = 0.6f;

    #endregion

    #region State

    private GravityBody _gravityBody;
    private Rigidbody _rigidbody;
    private float _horizontal;
    private float _vertical;
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;

    #endregion

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _jumpForce = Mathf.Clamp(_jumpForce, 0f, 500f);
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        _isGrounded = _groundCheck != null && Physics.CheckBox(_groundCheck.position, _groundCheckBoxSize * 0.5f, _groundCheck.rotation, _groundMask);

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
            Vector3 targetVel = desired * (_speed * moveMultiplier);
            Vector3 currentHorizVel = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, gravityUp);
            Vector3 velChange = targetVel - currentHorizVel;
            velChange = Vector3.ProjectOnPlane(velChange, gravityUp);
            _rigidbody.AddForce(velChange, ForceMode.VelocityChange);
        }

        if (turnInput)
        {
            float angle = _horizontal * (_turnSpeed * Time.fixedDeltaTime) * turnMultiplier;
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

    #region Jump & Dash

    private void Jump()
    {
        _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
        //_animator.SetTrigger("Jump");
    }

    private IEnumerator PerformDash()
    {
        _isDashing = true;
        //_animator.SetTrigger("Dash");

        Vector3 dashDirection = transform.forward.normalized;
        _rigidbody.AddForce(dashDirection * _dashForce, ForceMode.VelocityChange);

        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
        _dashTimer = _dashCooldown;
    }

    private void UpdateDashCooldown()
    {
        if (_dashTimer > 0f)
            _dashTimer -= Time.deltaTime;
    }

    #endregion

    private void UpdateAnimatorParameters()
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, -_gravityBody.GravityDirection);
        float speed = horizontalVelocity.magnitude;

        _animator.SetFloat("Speed", speed);
        //_animator.SetBool("IsGrounded", _isGrounded); 
    }

    void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(_groundCheck.position, _groundCheck.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _groundCheckBoxSize);
        Gizmos.matrix = old;
    }
}