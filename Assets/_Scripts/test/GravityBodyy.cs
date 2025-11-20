using UnityEngine;

public class GravityBodyy : MonoBehaviour
{
    public Transform planet;               // Transform planety
    public float gravity = 20f;            // si³a grawitacji
    public float alignSpeed = 5f;          // szybkoœæ obracania gracza

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;             // wy³¹czamy Unity gravity
    }

    void FixedUpdate()
    {
        if (planet == null) return;

        Vector3 gravityDirection = (planet.position - transform.position).normalized;

        // Przyci¹ganie
        rb.AddForce(gravityDirection * gravity, ForceMode.Acceleration);

        // Wyrównanie gracza (¿eby stopami by³ do planety)
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
    }
}
