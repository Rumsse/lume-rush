using UnityEngine;
using System;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour, IPlacementReceiver
{
    #region Properties 

    public static Action<PumpState> PumpStateChanged;
    public static PumpState CurrentPumpState { get; private set; }
    public bool HasPump => hasPump;

    [SerializeField] PlacementController placement;
    [SerializeField] GameObject pumpPrefab;
    [SerializeField] bool startWithPump = true;
    [SerializeField] string pumpTag = "Pump";
    [SerializeField] bool debugLogs = true;

    InteractionDetector interactionDetector;
    SpaceShipStorage nearbyShipStorage;

    #endregion

    #region State

    bool hasPump;
    bool pumpUsedUp;
    GameObject placedPump;
    int carriedPumpFuel;

    bool collecting;
    LongerInteractable collectingTarget;
    float collectTimer;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        interactionDetector = GetComponent<InteractionDetector>();
        if (!placement) placement = GetComponent<PlacementController>();
        if (!placement) placement = FindFirstObjectByType<PlacementController>();
        hasPump = startWithPump;
        NotifyPumpState();
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

        if (Input.GetKeyDown(KeyCode.Tab) && nearbyShipStorage != null) nearbyShipStorage.DebugShipInventory();

        if (Input.GetKeyDown(KeyCode.Q))
            ReturnToShip();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = other.GetComponent<SpaceShipStorage>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = null;
    }

    #endregion

    #region Interaction

    void ImmediatePickUp(Interactable interactable)
    {
        var go = interactable.gameObject;
        if (go.CompareTag(pumpTag))
        {
            if (pumpUsedUp) { if (debugLogs) Debug.Log("[PI] Pump already used up"); return; }
            var collector = go.GetComponent<PumpFuelCollector>();
            if (collector) carriedPumpFuel += collector.ConsumeFuel();
            hasPump = true;
            if (placedPump == go) placedPump = null;
            go.SetActive(false);
            NotifyPumpState();
            if (debugLogs) Debug.Log($"[PI] Picked up pump fuel={carriedPumpFuel}");
            return;
        }

        var resourceData = interactable.resourceData;
        if (resourceData == null) return;

        if (ToolbarManager.Instance.AddResource(resourceData.resourceName, resourceData.resourceIcon))
        {
            go.SetActive(false);
            if (debugLogs) Debug.Log($"[PI] Picked up: {resourceData.resourceName}");
        }
        else if (debugLogs) Debug.Log($"[PI] Toolbar full for: {resourceData.resourceName}");
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
            if (pumpUsedUp) { collectingTarget = null; return; }
            var collector = go.GetComponent<PumpFuelCollector>();
            if (collector) carriedPumpFuel += collector.ConsumeFuel();
            hasPump = true;
            if (placedPump == go) placedPump = null;
            go.SetActive(false);
            NotifyPumpState();
            if (debugLogs) Debug.Log($"[PI] Timed collect pump fuel={carriedPumpFuel}");
        }
        else
        {
            var resourceData = collectingTarget.resourceData;
            if (resourceData != null && ToolbarManager.Instance.AddResource(resourceData.resourceName, resourceData.resourceIcon))
            {
                go.SetActive(false);
                if (debugLogs) Debug.Log($"[PI] Timed collect: {resourceData.resourceName}");
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

    #endregion

    #region Ship Storage

    void DepositItemsToShip()
    {
        var toolbarSlots = ToolbarManager.Instance.GetSlots();
        nearbyShipStorage.AddToStorage(toolbarSlots);

        if (carriedPumpFuel > 0)
        {
            nearbyShipStorage.AddFuel(carriedPumpFuel);
            carriedPumpFuel = 0;
            pumpUsedUp = true;
            hasPump = false;
            placedPump = null;
            NotifyPumpState();
            if (debugLogs) Debug.Log("[PI] Deposited pump fuel; pump disabled for future placement");
        }
    }

    #endregion

    #region Pump Placement

    void BeginPumpPlacement()
    {
        if (!placement) { if (debugLogs) Debug.Log("[PI] No PlacementController"); return; }
        if (!pumpPrefab) { if (debugLogs) Debug.Log("[PI] No pump prefab"); return; }
        if (pumpUsedUp) { if (debugLogs) Debug.Log("[PI] Pump placement disabled"); return; }
        if (!hasPump) { if (debugLogs) Debug.Log("[PI] No pump to place"); return; }
        if (placedPump) { if (debugLogs) Debug.Log("[PI] Pump already placed"); return; }
        placement.BeginPlacement(pumpPrefab, this);
    }

    public void OnPlaced(GameObject instance)
    {
        placedPump = instance;
        hasPump = false;
        carriedPumpFuel = 0;
        instance.tag = pumpTag;

        instance.transform.Rotate(0f, 180f, 0f);

        var collector = instance.GetComponent<PumpFuelCollector>();
        if (collector) collector.BeginCollect();

        NotifyPumpState();
        if (debugLogs) Debug.Log("[PI] Pump placed");
    }

    void NotifyPumpState()
    {
        if (!hasPump)
            CurrentPumpState = PumpState.None;
        else if (carriedPumpFuel > 0)
            CurrentPumpState = PumpState.Full;
        else
            CurrentPumpState = PumpState.Empty;

        PumpStateChanged?.Invoke(CurrentPumpState);
    }

    #endregion

    #region Scene Management

    void ReturnToShip()
    {
        if (SpaceShipStorage.Instance != null)
            DepositItemsToShip();

        SceneLoader.Instance?.LoadShip();
    }

    #endregion
}

public enum PumpState
{
    None,
    Empty,
    Full
}