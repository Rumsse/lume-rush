using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomDetailsPanel : MonoBehaviour
{
    #region UI References

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button actionButton;

    #endregion

    #region Variables

    [SerializeField] private RoomDefinition roomToBuildPrototype;

    private RoomSlot activeSlot;

    #endregion

    private void Awake() => actionButton.onClick.AddListener(OnActionButtonClicked);

    public void ShowPanel(RoomSlot slot)
    {
        if (slot == null)
        {
            Debug.LogError("RoomSlot reference is null");
            return;
        }

        activeSlot = slot;
        gameObject.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (activeSlot.CurrentRoomType == RoomType.None)
            SetupBuildView();
        else
            SetupUpgradeView();
    }

    private void SetupBuildView()
    {
        titleText.text = "Empty Room";
        actionButton.interactable = true;

        if (roomToBuildPrototype == null)
        {
            Debug.LogError("Room prototype missing");
            return;
        }

        int buildCost = roomToBuildPrototype.levels[0].cost;
        costText.text = $"Build Cost: {buildCost}";
    }

    private void SetupUpgradeView()
    {
        titleText.text = $"{activeSlot.CurrentRoomType} (Lvl {activeSlot.CurrentLevel})";

        RoomDefinition def = activeSlot.CurrentDefinition;

        if (def == null)
        {
            Debug.LogError("Room definition missing on active slot");
            return;
        }

        if (activeSlot.CurrentLevel >= def.levels.Length)
        {
            costText.text = "Max Level Reached";
            actionButton.interactable = false;
            return;
        }

        actionButton.interactable = true;
        int upgradeCost = def.levels[activeSlot.CurrentLevel].cost;
        costText.text = $"Upgrade Cost: {upgradeCost}";
    }

    private void OnActionButtonClicked()
    {
        if (activeSlot.CurrentRoomType == RoomType.None)
            activeSlot.BuildOrUpgradeRoom(roomToBuildPrototype);
        else
            activeSlot.BuildOrUpgradeRoom(activeSlot.CurrentDefinition);

        RefreshUI();
    }
}