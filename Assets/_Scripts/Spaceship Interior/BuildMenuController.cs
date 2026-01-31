using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMenuController : MonoBehaviour
{
    public GameObject menuUI;

    SlotController currentSlot;

    public int titanium = 10;
    public int aluminium = 11;
    public int iron = 12;
    public int coal = 13;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        IsOpen = false;
        menuUI.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }
    public void Open(SlotController slot, Vector2 localPosition)
    {
        Debug.Log(">>> BuildMenu.Open CALLED <<<");

        if (slot == null || !slot.IsEmpty)
        {
            Debug.Log("Open ABORTED | slot null or not empty");
            return;
        }

        IsOpen = true;
        currentSlot = slot;

        menuUI.SetActive(true);

        RectTransform rect = menuUI.GetComponent<RectTransform>();
        rect.anchoredPosition = localPosition;

        Vector2 offset = new Vector2(20f, -20f);
        rect.anchoredPosition = localPosition + offset;

        foreach (var button in menuUI.GetComponentsInChildren<BuildOptionButton>(true))
        {
            Debug.Log("Refreshing: " + button.name);
            button.Refresh(this);
        }
    }

    public void Close()
    {
        IsOpen = false;
        currentSlot = null;
        menuUI.SetActive(false);
    }

    public bool CanAfford(
        int titaniumCost,
        int aluminiumCost,
        int ironCost,
        int coalCost)
    {
        return titanium >= titaniumCost
            && aluminium >= aluminiumCost
            && iron >= ironCost
            && coal >= coalCost;
    }

    public void Build(
    SlotDataSO data,
    int titaniumCost,
    int aluminiumCost,
    int ironCost,
    int coalCost)
    {
        Debug.Log($"BUILD CALLED | Slot: {currentSlot?.name} | Data: {data?.stationName}");

        if (!IsOpen || currentSlot == null)
            return;

        if (!CanAfford(titaniumCost, aluminiumCost, ironCost, coalCost))
            return;

        titanium -= titaniumCost;
        aluminium -= aluminiumCost;
        iron -= ironCost;
        coal -= coalCost;

        currentSlot.BuildStation(data);
        Close();
    }
}