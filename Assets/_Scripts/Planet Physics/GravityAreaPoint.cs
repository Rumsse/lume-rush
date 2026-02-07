using UnityEngine;

public class GravityAreaPoint : GravityArea
{
    [SerializeField] private Vector3 center;

    public override Vector3 GetGravityDirection(GravityBody gravityBody) =>
        (center - gravityBody.transform.position).normalized;
}