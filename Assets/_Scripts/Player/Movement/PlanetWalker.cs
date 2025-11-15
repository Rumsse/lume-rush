using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInput))]
public class PlanetWalker : MonoBehaviour
{
    #region Inspector
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotationSpeed = 12f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] LayerMask planetMask;
    [Header("Jump")]
    [SerializeField] float jumpImpulse = 6f;
    [SerializeField] float holdExtraAccel = 18f;
    [SerializeField] float maxHoldTime = 0.28f;
    [SerializeField] float groundCheckDistance = 0.12f;
    #endregion

    #region Cached
    Rigidbody rb;
    CapsuleCollider capsule;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    #endregion

    #region State
    Planet triggerPlanet;
    Vector2 moveInput;
    bool jumpPressed;
    bool isJumping;
    float holdTimer;
    RaycastHit groundHit;
    float halfHeight;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        playerInput = GetComponent<PlayerInput>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        halfHeight = Mathf.Max(0.01f, capsule.height * 0.5f);
        if (playerInput != null && playerInput.actions != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }
    }

    void Update()
    {
        ReadInput();
        var planet = GetPlanetForGravity();
        if (planet != null)
        {
            UpdateGrounded(planet);
            if (!isJumping && groundHit.collider != null && jumpPressed) BeginJump(planet);
            if (groundHit.collider != null && !isJumping) StickToSurface(planet);
        }
    }

    void FixedUpdate()
    {
        var planet = GetPlanetForGravity();
        if (planet != null)
        {
            ApplyGravity(planet);
            ApplyMovementFixed(planet);
            if (isJumping) HandleJumpHoldFixed(planet);
        }
        else rb.AddForce(Physics.gravity, ForceMode.Acceleration);
    }

    void ReadInput()
    {
        moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jumpPressed = jumpAction != null ? jumpAction.ReadValue<float>() > 0.5f : Keyboard.current.spaceKey.isPressed;
    }

    Planet GetPlanetForGravity()
    {
        if (triggerPlanet != null) return triggerPlanet;
        var list = Planet.All;
        if (list == null || list.Count == 0) return null;
        var nearest = list.OrderBy(p => (p.Center - transform.position).sqrMagnitude).FirstOrDefault();
        if (nearest == null) return null;
        var d = Vector3.Distance(transform.position, nearest.Center);
        return d <= nearest.GravityRadius ? nearest : null;
    }

    void ApplyGravity(Planet planet) => rb.AddForce((planet.Center - transform.position).normalized * gravity, ForceMode.Acceleration);

    void ApplyMovementFixed(Planet planet)
    {
        var surfaceNormal = (transform.position - planet.Center).normalized;
        var right = Vector3.Cross(surfaceNormal, transform.forward).normalized;
        var forward = Vector3.Cross(right, surfaceNormal).normalized;
        var desired = (forward * moveInput.y + right * moveInput.x);
        if (desired.sqrMagnitude > 1f) desired.Normalize();

        if (groundHit.collider != null && !isJumping)
        {
            var dir = (transform.position - planet.Center).normalized;
            var targetPos = planet.Center + dir * (planet.SurfaceRadius + halfHeight);
            var moveDelta = desired * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos + moveDelta);
            if (moveDelta.sqrMagnitude > 0.0001f)
            {
                var targetRot = Quaternion.LookRotation(moveDelta.normalized, surfaceNormal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            var tangentialVel = Vector3.ProjectOnPlane(rb.linearVelocity, (transform.position - planet.Center).normalized);
            var desiredVel = desired * moveSpeed;
            var accel = (desiredVel - tangentialVel) * 3f;
            rb.AddForce(accel, ForceMode.Acceleration);
        }
    }

    void StickToSurface(Planet planet)
    {
        var dir = (transform.position - planet.Center).normalized;
        var targetPos = planet.Center + dir * (planet.SurfaceRadius + halfHeight);
        rb.MovePosition(targetPos);
        rb.linearVelocity = Vector3.Project(rb.linearVelocity, dir) * 0f;
        var upRot = Quaternion.FromToRotation(transform.up, dir) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, upRot, rotationSpeed * 0.5f * Time.fixedDeltaTime));
    }

    void UpdateGrounded(Planet planet)
    {
        var surfaceNormal = (transform.position - planet.Center).normalized;
        var r = new Ray(transform.position, -surfaceNormal);
        var checkDist = halfHeight + groundCheckDistance;
        Physics.Raycast(r, out groundHit, checkDist, planetMask, QueryTriggerInteraction.Ignore);
        if (groundHit.collider != null) { isJumping = false; holdTimer = 0f; }
        else groundHit = new RaycastHit();
    }

    void BeginJump(Planet planet) { isJumping = true; holdTimer = 0f; rb.AddForce((transform.position - planet.Center).normalized * jumpImpulse, ForceMode.Impulse); }

    void HandleJumpHoldFixed(Planet planet)
    {
        if (!jumpPressed || holdTimer >= maxHoldTime) return;
        holdTimer += Time.fixedDeltaTime;
        rb.AddForce((transform.position - planet.Center).normalized * holdExtraAccel, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other) { var p = other.GetComponent<Planet>(); if (p != null) triggerPlanet = p; }
    void OnTriggerStay(Collider other) { var p = other.GetComponent<Planet>(); if (p != null) triggerPlanet = p; }
    void OnTriggerExit(Collider other) { var p = other.GetComponent<Planet>(); if (p != null && p == triggerPlanet) triggerPlanet = null; }
}