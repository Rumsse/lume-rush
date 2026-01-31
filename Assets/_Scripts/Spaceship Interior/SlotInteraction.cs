using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SlotInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private SlotPanelController slotPanel;
    [SerializeField] private BuildMenuController buildMenu;
    [SerializeField] private Canvas uiCanvas;

    [Header("Raycast")]
    [SerializeField] private LayerMask buildSlotMask;
    [SerializeField] private float rayDistance = 100f;

    private SlotController hoveredSlot;

    void Update()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        HandleClick(ray);

        if (buildMenu.IsOpen)
            return;

        HandleHover(ray);
    }

    void HandleClick(Ray ray)
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, buildSlotMask))
            return;

        SlotController slot = hit.collider.GetComponent<SlotController>();
        if (slot == null || !slot.IsEmpty)
            return;

        Vector2 clickPos = GetMouseLocalPosition();

        ClearHover();
        slotPanel.Hide();

        buildMenu.Open(slot, clickPos);
    }

    void HandleHover(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, buildSlotMask))
        {
            SlotController slot = hit.collider.GetComponent<SlotController>();

            if (slot != null && slot != hoveredSlot)
            {
                ClearHover();
                hoveredSlot = slot;
                hoveredSlot.SetSelected(true);
                slotPanel.Show(hoveredSlot);
            }
        }
        else
        {
            ClearHover();
        }
    }

    void ClearHover()
    {
        if (hoveredSlot == null)
            return;

        hoveredSlot.SetSelected(false);
        slotPanel.Hide();
        hoveredSlot = null;
    }

    Vector2 GetMouseLocalPosition()
    {
        RectTransform canvasRect = uiCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Mouse.current.position.ReadValue(),
            uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : uiCanvas.worldCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}