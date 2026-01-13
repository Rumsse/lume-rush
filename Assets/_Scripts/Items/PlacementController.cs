using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [SerializeField] Camera cam;

    [Header("Masks")]
    [SerializeField] LayerMask placementMask = ~0;
    [SerializeField] LayerMask obstacleMask = ~0;
    [SerializeField] LayerMask shipMask = 0;

    [Header("Placement Params")]
    [SerializeField] float maxPlaceDistance = 6f;
    [SerializeField] float placeLift = 0.02f;
    [SerializeField] Vector3 checkHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] float minShipDistance = 3f;
    [SerializeField] string shipTag = "SpaceShip";

    [Header("Probe/Ray Params")]
    [SerializeField] float rayDownOffset = 0.6f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] float groundProbeUp = 2f;
    [SerializeField] float groundProbeDown = 6f;
    [SerializeField] float forwardProbe = 2f;

    [Header("Preview Materials")]
    [SerializeField] Material canPlaceMaterial;
    [SerializeField] Material blockedMaterial;

    [SerializeField] bool debugLogs = true;

    GameObject currentPrefab;
    GameObject ghost;
    Renderer[] ghostRenderers;
    bool placing;
    bool canPlace;
    bool lastPreviewAllowed;
    Vector3 placePos;
    Quaternion placeRot;

    readonly Collider[] shipBuffer = new Collider[8];

    public bool IsPlacing => placing;

    void Update()
    {
        if (!placing) return;

        UpdatePlacementPreview();

        if (Input.GetMouseButtonDown(0) && canPlace) ConfirmPlacement();
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();
    }

    public void BeginPlacement(GameObject prefab)
    {
        if (!EnsureCamera()) { if (debugLogs) Debug.Log("[PLC] No camera"); return; }
        if (!prefab) { if (debugLogs) Debug.Log("[PLC] Prefab is null"); return; }
        if (placing) return;

        currentPrefab = prefab;
        ghost = Instantiate(currentPrefab);

        var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer >= 0) ghost.layer = ignoreLayer;

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>(true);

        var cols = ghost.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;

        var bodies = ghost.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++) bodies[i].isKinematic = true;

        lastPreviewAllowed = !canPlace;
        ApplyGhostMaterial(false);

        placing = true;
        if (debugLogs) Debug.Log($"[PLC] BeginPlacement {currentPrefab.name}");
    }

    void UpdatePlacementPreview()
    {
        if (!TryGetPlacementHit(out var hit))
        {
            if (ghost && ghost.activeSelf) ghost.SetActive(false);
            canPlace = false;
            ApplyGhostMaterial(false);
            if (debugLogs) Debug.Log("[PLC] Raycast miss");
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
            ApplyGhostMaterial(canPlace);
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
            if (debugLogs) Debug.Log("[PLC] No camera, using player forward");
        }

        if (Physics.Raycast(new Ray(origin, dir), out hit, maxPlaceDistance, placementMask, QueryTriggerInteraction.Collide)) return true;

        var probeStart = transform.position + transform.forward.normalized * Mathf.Min(forwardProbe, maxPlaceDistance) + Vector3.up * groundProbeUp;
        if (Physics.Raycast(probeStart, Vector3.down, out hit, groundProbeUp + groundProbeDown, placementMask, QueryTriggerInteraction.Collide)) return true;

        hit = default;
        return false;
    }

    bool ValidatePlacement(Vector3 pos, Quaternion rot)
    {
        if (obstacleMask.value != 0)
        {
            var center = pos + rot * (Vector3.up * checkHalfExtents.y);
            var blocked = Physics.CheckBox(center, checkHalfExtents, rot, obstacleMask, QueryTriggerInteraction.Ignore);
            if (blocked) return false;
        }

        if (minShipDistance > 0f)
        {
            int count;
            if (shipMask.value != 0)
                count = Physics.OverlapSphereNonAlloc(pos, minShipDistance, shipBuffer, shipMask, QueryTriggerInteraction.Ignore);
            else
                count = Physics.OverlapSphereNonAlloc(pos, minShipDistance, shipBuffer, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var c = shipBuffer[i];
                if (!c) continue;
                if (shipMask.value != 0 || c.CompareTag(shipTag)) return false;
            }
        }

        return true;
    }

    void ApplyGhostMaterial(bool allowed)
    {
        if (ghostRenderers == null || ghostRenderers.Length == 0) return;
        if (lastPreviewAllowed == allowed) return;
        lastPreviewAllowed = allowed;

        var mat = allowed ? canPlaceMaterial : blockedMaterial;
        if (!mat) return;

        for (int r = 0; r < ghostRenderers.Length; r++)
        {
            var rend = ghostRenderers[r];
            if (!rend) continue;

            var mats = rend.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            rend.sharedMaterials = mats;
        }
    }

    void ConfirmPlacement()
    {
        Instantiate(currentPrefab, placePos, placeRot);
        if (debugLogs) Debug.Log("[PLC] ConfirmPlacement");
        CancelPlacement();
    }

    public void CancelPlacement()
    {
        placing = false;
        canPlace = false;
        currentPrefab = null;
        if (ghost) Destroy(ghost);
        ghost = null;
        ghostRenderers = null;
        if (debugLogs) Debug.Log("[PLC] CancelPlacement");
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
            var msg = "[PLC] No active Camera found. Cameras in scene: ";
            for (int i = 0; i < all.Length; i++) msg += $"{all[i].name}(active={all[i].isActiveAndEnabled}, tag={all[i].tag}) ";
            Debug.Log(msg);
        }

        return cam && cam.isActiveAndEnabled;
    }
}