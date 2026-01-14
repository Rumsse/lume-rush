using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public LayerMask InteractableLayer;
    public float DetectionRadius = 3f;
    public int MaxColliders = 16;

    public Interactable CurrentInteractable { get; private set; }

    Collider[] buffer;

    void Awake() => buffer = new Collider[MaxColliders];

    void Update()
    {
        var count = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, buffer, InteractableLayer);

        Interactable nearest = null;
        float minSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var col = buffer[i];
            if (col == null) continue;
            if (!col.TryGetComponent(out Interactable interactable)) continue;
            if (!interactable.CanBePickedUp) continue;

            var sqr = (col.transform.position - transform.position).sqrMagnitude;
            if (sqr < minSqr) { minSqr = sqr; nearest = interactable; }
        }

        if (nearest != CurrentInteractable)
        {
            if (CurrentInteractable != null) CurrentInteractable.SetHighlighted(false);
            CurrentInteractable = nearest;
            if (CurrentInteractable != null) CurrentInteractable.SetHighlighted(true);
        }
    }
}