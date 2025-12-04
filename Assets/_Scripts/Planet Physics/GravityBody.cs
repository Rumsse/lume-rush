using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private static float gravityForce = 800;

    public Vector3 GravityDirection
    {
        get
        {
            if (gravityAreas.Count == 0) return Vector3.zero;
            gravityAreas.Sort((area1, area2) => area1.Priority.CompareTo(area2.Priority));
            return gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    private Rigidbody rb;
    private List<GravityArea> gravityAreas;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        gravityAreas = new List<GravityArea>();
    }

    void FixedUpdate()
    {
        rb.AddForce(GravityDirection * (gravityForce * Time.fixedDeltaTime), ForceMode.Acceleration);

        if (GravityDirection != Vector3.zero)
            AlignToGravity();
    }

    private void AlignToGravity()
    {
        Vector3 gravityUp = -GravityDirection.normalized;
        Vector3 currentUp = transform.up;

        // P³ynna korekta rotacji do powierzchni planety
        Quaternion targetRotation = Quaternion.FromToRotation(currentUp, gravityUp) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
    }

    public void AddGravityArea(GravityArea gravityArea)
    {
        gravityAreas.Add(gravityArea);
    }

    public void RemoveGravityArea(GravityArea gravityArea)
    {
        gravityAreas.Remove(gravityArea);
    }
}