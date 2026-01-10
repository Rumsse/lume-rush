using UnityEngine;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour
{
    InteractionDetector interactionDetector;

    void Awake() => interactionDetector = GetComponent<InteractionDetector>();

    void Update()
    {
        if (interactionDetector.CurrentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            TryPickUp(interactionDetector.CurrentInteractable);
        }
    }

    void TryPickUp(Interactable interactable)
    {
        var itemData = interactable.itemData;
        if (itemData == null) return;

        if (ToolbarManager.Instance.AddItem(itemData.itemName, itemData.itemIcon))
        {
            interactable.gameObject.SetActive(false);
            Debug.Log($"Picked up item: {itemData.itemName}");
        }
        else
        {
            Debug.Log($"Cannot pick up item: {itemData.itemName}. Toolbar is full!");
        }
    }
}