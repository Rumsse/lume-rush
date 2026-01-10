using UnityEngine;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour
{
    InteractionDetector interactionDetector;
    SpaceShipStorage nearbyShipStorage;

    void Awake() => interactionDetector = GetComponent<InteractionDetector>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactionDetector.CurrentInteractable != null)
            {
                TryPickUp(interactionDetector.CurrentInteractable);
            }
            else if (nearbyShipStorage != null)
            {
                DepositItemsToShip();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && nearbyShipStorage != null)
        {
            nearbyShipStorage.DebugShipInventory(); 
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

    void DepositItemsToShip()
    {
        var toolbarSlots = ToolbarManager.Instance.GetSlots();
        nearbyShipStorage.AddToStorage(toolbarSlots);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpaceShip"))
        {
            nearbyShipStorage = other.GetComponent<SpaceShipStorage>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpaceShip"))
        {
            nearbyShipStorage = null;
        }
    }
}