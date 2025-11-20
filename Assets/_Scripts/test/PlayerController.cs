using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Transform planet;
    public float moveSpeed = 6f;
    public float jumpForce = 6f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private float h, v;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if (planet == null) return;

        // Normalna powierzchni planety
        Vector3 gravityUp = (transform.position - planet.position).normalized;

        // Własny "forward" po powierzchni planety
        Vector3 moveForward = Vector3.Cross(transform.right, gravityUp).normalized;
        Vector3 moveRight = Vector3.Cross(gravityUp, moveForward).normalized;

        Vector3 moveDir = (moveForward * v + moveRight * h).normalized;

        // RUCH — addforce dla płynności i działania na sferze
        rb.AddForce(moveDir * moveSpeed, ForceMode.Acceleration);

        // SKOK
        if (Input.GetButton("Jump") && IsGrounded())
        {
            rb.AddForce(gravityUp * jumpForce, ForceMode.VelocityChange);
        }

        // GRAWITACJA
        rb.AddForce(-gravityUp * 30f, ForceMode.Acceleration);

        // OBRÓT GRACZA do planety + kierunku ruchu
        Quaternion targetRot;

        if (moveDir.sqrMagnitude > 0.1f)
        {
            targetRot = Quaternion.LookRotation(moveDir, gravityUp);
        }
        else
        {
            targetRot = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, out _, 2.5f);
    }
}
