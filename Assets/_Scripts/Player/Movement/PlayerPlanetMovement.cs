using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanetMovement : MonoBehaviour
{
    #region Inspector

    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;

    [SerializeField] public float _speed = 15f;
    [SerializeField] public float _turnSpeed = 1500f;
    [SerializeField] private Vector3 _groundCheckBoxSize = new Vector3(1.2f, 0.2f, 1.8f);
    [SerializeField] private float _jumpForce = 500f;

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

    #endregion

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        _isGrounded = _groundCheck != null && Physics.CheckBox(_groundCheck.position, _groundCheckBoxSize * 0.5f, _groundCheck.rotation, _groundMask);

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
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