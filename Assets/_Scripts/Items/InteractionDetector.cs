using UnityEngine;
using System.Linq;

public class InteractionDetector : MonoBehaviour
{
    public LayerMask InteractableLayer;
    public float DetectionRadius = 3f;

    public Interactable CurrentInteractable { get; private set; }

    void Update()
    {
        var colliders = Physics.OverlapSphere(transform.position, DetectionRadius, InteractableLayer);

        CurrentInteractable = colliders.Length > 0
            ? colliders.Select(collider => collider.GetComponent<Interactable>()).FirstOrDefault(interactable => interactable != null)
            : null;
    }
}