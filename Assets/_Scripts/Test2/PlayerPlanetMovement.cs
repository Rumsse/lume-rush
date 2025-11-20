using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanetMovement : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;

    private float _groundCheckRadius = 0.3f;
    public float _speed = 15f;
    public float _turnSpeed = 1500f;
    private float _jumpForce = 500f;

    private GravityBody _gravityBody;
    private Rigidbody _rigidbody;
    private Vector3 _direction;
    private bool _isGrounded;


    void Start()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();
        _gravityBody = transform.GetComponent<GravityBody>();

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        _direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        bool isRunning = _direction.magnitude > 0.1f;

        if (isRunning && _isGrounded)
        {
            // U�yj lokalnych kierunk�w zamiast globalnych
            Vector3 moveDirection = transform.TransformDirection(new Vector3(_direction.x, 0, _direction.z));

            // Ruch z interpolacj� - mniej szarpni��
            Vector3 targetVelocity = moveDirection * _speed;
            Vector3 velocityChange = targetVelocity - _rigidbody.linearVelocity;

            // Ogranicz Y �eby nie wp�ywa� na grawitacj�
            velocityChange.y = 0;

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

            // P�ynniejsze obracanie
            if (_direction.x != 0)
            {
                float turnAmount = _direction.x * (_turnSpeed * Time.fixedDeltaTime);
                Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
                _rigidbody.MoveRotation(_rigidbody.rotation * turnRotation);
            }
        }

        // Dodatkowe t�umienie pr�dko�ci gdy nie porusza si�
        if (!isRunning && _isGrounded)
        {
            Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(-horizontalVelocity * 5f, ForceMode.Acceleration);
        }
    }

    // Wizualna pomoc - pokazuje czy gracz jest uziemiony
    void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }

}