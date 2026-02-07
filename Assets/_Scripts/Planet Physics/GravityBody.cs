using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    public GravityArea CurrentGravityArea { get; private set; }

    public Vector3 GravityDirection
    {
        get
        {
            UpdateCurrentGravityArea();
            if (CurrentGravityArea == null) return Vector3.zero;
            return CurrentGravityArea.GetGravityDirection(this).normalized;
        }
    }

    public float CurrentGravityForce
    {
        get
        {
            UpdateCurrentGravityArea();
            return CurrentGravityArea != null ? CurrentGravityArea.GravityForce : 0f;
        }
    }

    private Rigidbody rb;
    private List<GravityArea> gravityAreas;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start() => gravityAreas = new List<GravityArea>();

    void FixedUpdate()
    {
        if (CurrentGravityArea != null)
        {
            rb.AddForce(GravityDirection * (CurrentGravityForce * Time.fixedDeltaTime), ForceMode.Acceleration);
            AlignToGravity();
        }
    }

    void AlignToGravity()
    {
        Vector3 gravityUp = -GravityDirection.normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
    }

    void UpdateCurrentGravityArea()
    {
        if (gravityAreas.Count == 0)
        {
            CurrentGravityArea = null;
            return;
        }

        CurrentGravityArea = gravityAreas.OrderBy(area => area.Priority).Last();
    }

    public void AddGravityArea(GravityArea gravityArea)
    {
        gravityAreas.Add(gravityArea);
        UpdateCurrentGravityArea();
    }

    public void RemoveGravityArea(GravityArea gravityArea)
    {
        gravityAreas.Remove(gravityArea);
        UpdateCurrentGravityArea();
    }
}