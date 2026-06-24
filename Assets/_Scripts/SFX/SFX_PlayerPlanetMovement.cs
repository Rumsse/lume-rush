using UnityEngine;
using FMODUnity;

public class PlayerPlanetMovementSFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPlanetMovement _movement;

    [Header("Ground SFX")]
    [SerializeField] private EventReference _groundFootstepSound;
    [SerializeField] private EventReference _groundJumpSound;
    [SerializeField] private EventReference _groundLandSound;

    [Header("Water SFX")]
    [SerializeField] private EventReference _waterFootstepSound;
    [SerializeField] private EventReference _waterJumpSound;
    [SerializeField] private EventReference _waterLandSound;

    [Header("Dash SFX")]
    [SerializeField] private EventReference _dashSound;

    [Header("Footsteps")]
    [SerializeField] private float _stepInterval = 0.35f;
    [SerializeField] private float _minSpeedForSteps = 1f;

    private float _stepTimer;
    private bool _wasGrounded;
    private bool _wasOnWater;

    private void Awake()
    {
        if (_movement == null)
            _movement = GetComponent<PlayerPlanetMovement>();
    }

    private void Start()
    {
        if (_movement == null)
            return;

        _wasGrounded = _movement.IsGrounded;
        _wasOnWater = _movement.IsOnWater;
    }

    private void Update()
    {
        if (_movement == null)
            return;

        HandleFootsteps();
        HandleLanding();

        _wasGrounded = _movement.IsGrounded;
        _wasOnWater = _movement.IsOnWater;
    }

    public void PlayJumpSound()
    {
        EventReference jumpSound = _movement.IsOnWater
            ? _waterJumpSound
            : _groundJumpSound;

        RuntimeManager.PlayOneShot(jumpSound, transform.position);
    }

    public void PlayDashSound()
    {
        RuntimeManager.PlayOneShot(_dashSound, transform.position);
    }

    private void HandleFootsteps()
    {
        if (!_movement.IsGrounded)
        {
            _stepTimer = 0f;
            return;
        }

        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(_movement.Velocity, -_movement.GravityDirection);

        if (horizontalVelocity.magnitude < _minSpeedForSteps)
        {
            _stepTimer = 0f;
            return;
        }

        _stepTimer -= Time.deltaTime;

        if (_stepTimer <= 0f)
        {
            EventReference footstepSound = _movement.IsOnWater
                ? _waterFootstepSound
                : _groundFootstepSound;

            RuntimeManager.PlayOneShot(footstepSound, transform.position);

            _stepTimer = _stepInterval;
        }
    }

    private void HandleLanding()
    {
        if (!_wasGrounded && _movement.IsGrounded)
        {
            EventReference landSound = _movement.IsOnWater
                ? _waterLandSound
                : _groundLandSound;

            RuntimeManager.PlayOneShot(landSound, transform.position);
        }
    }
}