using System.Collections;
using UnityEngine;

public class ShipMarker : MonoBehaviour
{
    #region Inspector

    [SerializeField] RectTransform markerRect;
    [SerializeField] string shipTag = "SpaceShip";
    [SerializeField] string playerTag = "Player";
    [SerializeField] float hideDistance = 15f;
    [SerializeField] float padding = 50f;

    #endregion

    #region State

    Camera mainCamera;
    Transform targetShip;
    Transform player;
    RectTransform canvasRect;
    bool targetsReady;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        mainCamera = Camera.main;

        if (markerRect != null)
        {
            var canvas = markerRect.GetComponentInParent<Canvas>();

            if (canvas != null)
                canvasRect = canvas.rootCanvas.GetComponent<RectTransform>();

            markerRect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    void Start()
    {
        if (markerRect)
            markerRect.gameObject.SetActive(false);

        StartCoroutine(FindTargets());
    }

    void Update()
    {
        if (!targetsReady)
            return;

        if (player == null || targetShip == null)
        {
            targetsReady = false;
            StartCoroutine(FindTargets());

            if (markerRect && markerRect.gameObject.activeSelf)
                markerRect.gameObject.SetActive(false);

            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || canvasRect == null || markerRect == null)
            return;

        UpdateMarkerState();
    }

    #endregion

    #region Logic

    IEnumerator FindTargets()
    {
        while (player == null || targetShip == null)
        {
            if (player == null)
            {
                var pObj = GameObject.FindGameObjectWithTag(playerTag);

                if (pObj != null)
                    player = pObj.transform;
            }

            if (targetShip == null)
            {
                var sObj = GameObject.FindGameObjectWithTag(shipTag);

                if (sObj != null)
                    targetShip = sObj.transform;
            }

            yield return new WaitForSeconds(0.5f);
        }

        targetsReady = true;
    }

    void UpdateMarkerState()
    {
        var distance = Vector3.Distance(player.position, targetShip.position);

        if (distance <= hideDistance)
        {
            if (markerRect.gameObject.activeSelf)
                markerRect.gameObject.SetActive(false);

            return;
        }

        if (!markerRect.gameObject.activeSelf)
            markerRect.gameObject.SetActive(true);

        UpdateMarkerPositionAndRotation();
    }

    void UpdateMarkerPositionAndRotation()
    {
        var tangentDir = Vector3.ProjectOnPlane(targetShip.position - player.position, player.up).normalized;

        var playerScreenPos = mainCamera.WorldToScreenPoint(player.position);
        var targetScreenPos = mainCamera.WorldToScreenPoint(player.position + tangentDir);

        var direction = new Vector2(targetScreenPos.x - playerScreenPos.x, targetScreenPos.y - playerScreenPos.y).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        markerRect.localEulerAngles = new Vector3(0f, 0f, angle);

        var bounds = new Vector2((canvasRect.rect.width / 2f) - padding, (canvasRect.rect.height / 2f) - padding);
        var finalPos = Vector2.zero;

        if (Mathf.Abs(direction.x) > 0.0001f)
        {
            var slope = direction.y / direction.x;

            if (Mathf.Abs(slope) > bounds.y / bounds.x)
            {
                finalPos.y = Mathf.Sign(direction.y) * bounds.y;
                finalPos.x = finalPos.y / slope;
            }
            else
            {
                finalPos.x = Mathf.Sign(direction.x) * bounds.x;
                finalPos.y = finalPos.x * slope;
            }
        }
        else
            finalPos.y = Mathf.Sign(direction.y) * bounds.y;

        var worldPos = canvasRect.TransformPoint(new Vector3(finalPos.x, finalPos.y, 0f));
        markerRect.position = worldPos;
    }

    #endregion
}