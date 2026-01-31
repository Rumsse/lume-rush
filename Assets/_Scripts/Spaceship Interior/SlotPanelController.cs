using UnityEngine;
using UnityEngine.InputSystem;


public class SlotPanelController : MonoBehaviour
{
    public GameObject panelUI;
    public StatsDisplay SD;
    public Canvas canvas;

    SlotController currentSlot;

    bool isLocked;


    void Update()
    {
        if (panelUI.activeSelf && !isLocked)
        {
            UpdatePanelPosition();
        }
    }

    public void LockPosition()
    {
        isLocked = true;
    }

    public void UnlockPosition()
    {
        isLocked = false;
    }


    public void Show(SlotController slot)
    {
        if (slot.stationData == null)
        {
            Hide();
            return;
        }


        if (currentSlot == slot) return;


        currentSlot = slot;
        panelUI.SetActive(true);
        SD.DisplayStats(slot.stationData);
    }


    public void Hide()
    {
        currentSlot = null;
        SD.Clear();
        panelUI.SetActive(false);
    }


    void UpdatePanelPosition()
    {
        RectTransform panelRect = panelUI.GetComponent<RectTransform>();
        Vector2 mousePos = Mouse.current.position.ReadValue();


        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvas.transform as RectTransform,
        mousePos,
        canvas.worldCamera,
        out Vector2 localPoint
        );


        panelRect.localPosition = Vector2.Lerp(
        panelRect.localPosition,
        localPoint + new Vector2(2f, -2f),
        Time.deltaTime * 15f
        );
    }

    public Vector2 GetCurrentPanelPosition()
    {
        RectTransform rect = panelUI.GetComponent<RectTransform>();
        return rect.localPosition;
    }
}