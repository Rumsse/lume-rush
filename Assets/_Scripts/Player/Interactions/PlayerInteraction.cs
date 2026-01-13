using UnityEngine;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour
{
    InteractionDetector interactionDetector;
    SpaceShipStorage nearbyShipStorage;

    [SerializeField] PlacementController placement;
    [SerializeField] GameObject pumpPrefab;
    [SerializeField] bool debugLogs = true;

    void Awake()
    {
        interactionDetector = GetComponent<InteractionDetector>();
        if (!placement) placement = GetComponent<PlacementController>();
        if (!placement) placement = FindFirstObjectByType<PlacementController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var interactable = interactionDetector.CurrentInteractable;
            if (interactable != null) TryPickUp(interactable);
            else if (nearbyShipStorage != null) DepositItemsToShip();
        }

        if (Input.GetKeyDown(KeyCode.Q) && nearbyShipStorage != null) nearbyShipStorage.DebugShipInventory();
        if (Input.GetKeyDown(KeyCode.F)) BeginPumpPlacement();
    }

    void TryPickUp(Interactable interactable)
    {
        var itemData = interactable.itemData;
        if (itemData == null) return;

        if (ToolbarManager.Instance.AddItem(itemData.itemName, itemData.itemIcon))
        {
            interactable.gameObject.SetActive(false);
            if (debugLogs) Debug.Log($"[PI] Picked up: {itemData.itemName}");
        }
        else if (debugLogs) Debug.Log($"[PI] Toolbar full for: {itemData.itemName}");
    }

    void DepositItemsToShip()
    {
        var toolbarSlots = ToolbarManager.Instance.GetSlots();
        nearbyShipStorage.AddToStorage(toolbarSlots);
    }

    void BeginPumpPlacement()
    {
        if (!placement) { if (debugLogs) Debug.Log("[PI] No PlacementController"); return; }
        if (!pumpPrefab) { if (debugLogs) Debug.Log("[PI] No pump prefab"); return; }
        placement.BeginPlacement(pumpPrefab);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = other.GetComponent<SpaceShipStorage>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = null;
    }
}