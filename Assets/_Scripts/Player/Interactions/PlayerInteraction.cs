using UnityEngine;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour, IPlacementReceiver
{
    InteractionDetector interactionDetector;
    SpaceShipStorage nearbyShipStorage;

    [SerializeField] PlacementController placement;
    [SerializeField] GameObject pumpPrefab;
    [SerializeField] bool startWithPump = true;
    [SerializeField] string pumpTag = "Pump";
    [SerializeField] bool debugLogs = true;

    bool hasPump;
    GameObject placedPump;

    bool collecting;
    LongerInteractable collectingTarget;
    float collectTimer;

    void Awake()
    {
        interactionDetector = GetComponent<InteractionDetector>();
        if (!placement) placement = GetComponent<PlacementController>();
        if (!placement) placement = FindFirstObjectByType<PlacementController>();
        hasPump = startWithPump;
    }

    void Update()
    {
        var interactable = interactionDetector.CurrentInteractable;

        if (Input.GetKeyDown(KeyCode.F)) BeginPumpPlacement();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactable != null)
            {
                if (interactable is LongerInteractable ti) BeginCollect(ti);
                else ImmediatePickUp(interactable);
            }
            else if (nearbyShipStorage != null) DepositItemsToShip();
        }

        if (collecting)
        {
            if (interactable != collectingTarget || !Input.GetKey(KeyCode.E)) CancelCollect();
            else
            {
                collectTimer += Time.deltaTime;
                if (collectTimer >= collectingTarget.CollectTime) CompleteCollect();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && nearbyShipStorage != null) nearbyShipStorage.DebugShipInventory();
    }

    void ImmediatePickUp(Interactable interactable)
    {
        var go = interactable.gameObject;
        if (go.CompareTag(pumpTag))
        {
            hasPump = true;
            if (placedPump == go) placedPump = null;
            go.SetActive(false);
            if (debugLogs) Debug.Log("[PI] Picked up pump");
            return;
        }

        var itemData = interactable.itemData;
        if (itemData == null) return;

        if (ToolbarManager.Instance.AddItem(itemData.itemName, itemData.itemIcon))
        {
            go.SetActive(false);
            if (debugLogs) Debug.Log($"[PI] Picked up: {itemData.itemName}");
        }
        else if (debugLogs) Debug.Log($"[PI] Toolbar full for: {itemData.itemName}");
    }

    void BeginCollect(LongerInteractable target)
    {
        if (collecting) return;
        collectingTarget = target;
        collectTimer = 0f;
        collecting = true;
        if (debugLogs) Debug.Log("[PI] Begin timed collect");
    }

    void CompleteCollect()
    {
        collecting = false;
        var go = collectingTarget.gameObject;

        if (go.CompareTag(pumpTag))
        {
            hasPump = true;
            if (placedPump == go) placedPump = null;
            go.SetActive(false);
            if (debugLogs) Debug.Log("[PI] Timed collect pump");
        }
        else
        {
            var itemData = collectingTarget.itemData;
            if (itemData != null && ToolbarManager.Instance.AddItem(itemData.itemName, itemData.itemIcon))
            {
                go.SetActive(false);
                if (debugLogs) Debug.Log($"[PI] Timed collect: {itemData.itemName}");
            }
            else if (debugLogs) Debug.Log("[PI] Timed collect failed");
        }

        collectingTarget = null;
    }

    void CancelCollect()
    {
        collecting = false;
        collectingTarget = null;
        collectTimer = 0f;
        if (debugLogs) Debug.Log("[PI] Cancel timed collect");
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
        if (!hasPump) { if (debugLogs) Debug.Log("[PI] No pump to place"); return; }
        if (placedPump) { if (debugLogs) Debug.Log("[PI] Pump already placed"); return; }
        placement.BeginPlacement(pumpPrefab, this);
    }

    public void OnPlaced(GameObject instance)
    {
        placedPump = instance;
        hasPump = false;
        instance.tag = pumpTag;
        if (debugLogs) Debug.Log("[PI] Pump placed");
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