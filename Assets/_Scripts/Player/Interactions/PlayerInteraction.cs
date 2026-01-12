using UnityEngine;

[RequireComponent(typeof(InteractionDetector))]
public class PlayerInteraction : MonoBehaviour
{
    InteractionDetector interactionDetector;
    SpaceShipStorage nearbyShipStorage;

    [SerializeField] Camera cam;

    [SerializeField] GameObject pumpPrefab;
    [SerializeField] LayerMask placementMask = ~0;
    [SerializeField] LayerMask obstacleMask = ~0;
    [SerializeField] float maxPlaceDistance = 6f;
    [SerializeField] float placeLift = 0.02f;
    [SerializeField] Vector3 pumpCheckHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField] string outlineThicknessProperty = "_OutlineThickness";
    [SerializeField] string outlineColorProperty = "_OutlineColor";
    [SerializeField] float previewOutlineThickness = 1.6f;
    [SerializeField] Color canPlaceColor = new Color(0.2f, 1f, 0.5f, 1f);
    [SerializeField] Color blockedColor = new Color(1f, 0.2f, 0.2f, 1f);

    [SerializeField] float rayDownOffset = 0.6f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] float groundProbeUp = 2f;
    [SerializeField] float groundProbeDown = 6f;
    [SerializeField] float forwardProbe = 2f;
    [SerializeField] bool debugLogs = true;

    GameObject ghost;
    Renderer[] ghostRenderers;
    MaterialPropertyBlock mpb;
    int thicknessId;
    int colorId;
    bool placing;
    bool canPlace;
    Vector3 placePos;
    Quaternion placeRot;

    void Awake()
    {
        interactionDetector = GetComponent<InteractionDetector>();
        EnsureCamera();
        mpb = new MaterialPropertyBlock();
        thicknessId = Shader.PropertyToID(outlineThicknessProperty);
        colorId = Shader.PropertyToID(outlineColorProperty);
        if (debugLogs) Debug.Log($"[PI] Awake cam={(cam ? cam.name : "null")} pump={(pumpPrefab ? pumpPrefab.name : "null")}");
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
        if (Input.GetKeyDown(KeyCode.F)) BeginPlacement();

        if (!placing) return;

        UpdatePlacementPreview();

        if (Input.GetMouseButtonDown(0) && canPlace) ConfirmPlacement();
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = other.GetComponent<SpaceShipStorage>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpaceShip")) nearbyShipStorage = null;
    }

    void BeginPlacement()
    {
        EnsureCamera();
        if (!pumpPrefab) { if (debugLogs) Debug.Log("[PI] Pump prefab not assigned"); return; }
        if (!placing)
        {
            ghost = Instantiate(pumpPrefab);
            var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreLayer >= 0) ghost.layer = ignoreLayer;

            ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);

            var cols = ghost.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;

            var bodies = ghost.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++) bodies[i].isKinematic = true;

            placing = true;
            if (debugLogs) Debug.Log($"[PI] BeginPlacement cam={(cam ? cam.name : "null")}");
        }
    }

    void UpdatePlacementPreview()
    {
        if (!TryGetPlacementHit(out var hit))
        {
            if (ghost && ghost.activeSelf) ghost.SetActive(false);
            canPlace = false;
            if (debugLogs) Debug.Log("[PI] Raycast miss");
            return;
        }

        if (ghost && !ghost.activeSelf) ghost.SetActive(true);

        var forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        placeRot = forwardOnPlane.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(forwardOnPlane, hit.normal) : Quaternion.FromToRotation(Vector3.up, hit.normal);
        placePos = hit.point + hit.normal * placeLift;

        canPlace = ValidatePlacement(placePos, placeRot);

        if (ghost)
        {
            ghost.transform.SetPositionAndRotation(placePos, placeRot);
            SetGhostPreview(canPlace);
        }
    }

    bool TryGetPlacementHit(out RaycastHit hit)
    {
        Vector3 origin;
        Vector3 dir;

        if (cam && cam.isActiveAndEnabled)
        {
            origin = cam.transform.position - Vector3.up * rayDownOffset;
            dir = cam.transform.forward;
        }
        else
        {
            origin = transform.position + Vector3.up * eyeHeight;
            dir = transform.forward;
            if (debugLogs) Debug.Log("[PI] No camera, using player forward");
        }

        if (Physics.Raycast(new Ray(origin, dir), out hit, maxPlaceDistance, placementMask, QueryTriggerInteraction.Collide)) return true;

        var probeStart = transform.position + transform.forward.normalized * Mathf.Min(forwardProbe, maxPlaceDistance) + Vector3.up * groundProbeUp;
        if (Physics.Raycast(probeStart, Vector3.down, out hit, groundProbeUp + groundProbeDown, placementMask, QueryTriggerInteraction.Collide)) return true;

        hit = default;
        return false;
    }

    bool ValidatePlacement(Vector3 pos, Quaternion rot)
    {
        if (obstacleMask.value == 0) return true;
        var center = pos + rot * Vector3.up * pumpCheckHalfExtents.y;
        var blocked = Physics.CheckBox(center, pumpCheckHalfExtents, rot, obstacleMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    void SetGhostPreview(bool allowed)
    {
        if (ghostRenderers == null || ghostRenderers.Length == 0) return;

        mpb.Clear();
        mpb.SetFloat(thicknessId, previewOutlineThickness);
        mpb.SetColor(colorId, allowed ? canPlaceColor : blockedColor);

        for (int r = 0; r < ghostRenderers.Length; r++)
        {
            var rend = ghostRenderers[r];
            var mats = rend.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) rend.SetPropertyBlock(mpb, i);
        }
    }

    void ConfirmPlacement()
    {
        Instantiate(pumpPrefab, placePos, placeRot);
        if (debugLogs) Debug.Log("[PI] ConfirmPlacement");
        CancelPlacement();
    }

    void CancelPlacement()
    {
        placing = false;
        canPlace = false;
        if (ghost) Destroy(ghost);
        ghost = null;
        ghostRenderers = null;
        if (debugLogs) Debug.Log("[PI] CancelPlacement");
    }

    bool EnsureCamera()
    {
        if (cam && cam.isActiveAndEnabled) return true;

        cam = Camera.main && Camera.main.isActiveAndEnabled ? Camera.main : cam;
        if (cam && cam.isActiveAndEnabled) return true;

#if UNITY_2022_1_OR_NEWER
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        var cams = Camera.allCameras;
#endif
        for (int i = 0; i < cams.Length; i++)
            if (cams[i] && cams[i].isActiveAndEnabled) { cam = cams[i]; break; }

        if (!cam && debugLogs)
        {
#if UNITY_2022_1_OR_NEWER
            var all = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var all = GameObject.FindObjectsOfType<Camera>(true);
#endif
            var msg = "[PI] No active Camera found. Cameras in scene: ";
            for (int i = 0; i < all.Length; i++) msg += $"{all[i].name}(active={all[i].isActiveAndEnabled}, tag={all[i].tag}) ";
            Debug.Log(msg);
        }

        return cam && cam.isActiveAndEnabled;
    }
}