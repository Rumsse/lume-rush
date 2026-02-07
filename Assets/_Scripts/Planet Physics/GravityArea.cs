using UnityEngine;

public abstract class GravityArea : MonoBehaviour
{
    [SerializeField] private int priority;
    [SerializeField] private float gravityForce = 800f;

    public int Priority => priority;
    public float GravityForce => gravityForce;

    public void SetGravityForce(float force) => gravityForce = force;

    void Awake()
    {
        if (TryGetComponent<Collider>(out var collider))
            collider.isTrigger = true;
    }

    public abstract Vector3 GetGravityDirection(GravityBody gravityBody);

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
            gravityBody.AddGravityArea(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
            gravityBody.RemoveGravityArea(this);
    }
}