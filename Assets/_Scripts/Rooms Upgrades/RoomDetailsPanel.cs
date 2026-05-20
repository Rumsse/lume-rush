using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomDetailsPanel : MonoBehaviour
{
    #region UI References

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button chooseRoomButton;
    [SerializeField] private GameObject roomSelectionPanel;

    #endregion

    #region Variables

    private RoomSlot activeSlot;
    private RoomDefinition selectedPrototype;

    #endregion

    private void Awake()
    {
        actionButton.onClick.AddListener(OnActionButtonClicked);
        chooseRoomButton.onClick.AddListener(() => roomSelectionPanel.SetActive(true));
    }

    public void ShowPanel(RoomSlot slot)
    {
        if (slot == null)
        {
            Debug.LogError("RoomSlot reference is null");
            return;
        }

        activeSlot = slot;
        selectedPrototype = null;
        roomSelectionPanel.SetActive(false);
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void SelectRoomPrototype(RoomDefinition def)
    {
        selectedPrototype = def;
        roomSelectionPanel.SetActive(false);
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
        chooseRoomButton.gameObject.SetActive(true);

        if (selectedPrototype == null)
        {
            titleText.text = "Empty Room - Select Type";
            costText.text = "Select a room type to see costs.";
            actionButton.interactable = false;
            return;
        }

        titleText.text = $"Building: {selectedPrototype.roomType}";
        var costs = selectedPrototype.levels[0].costs;
        UpdateCostUIAndButton(costs, true);
    }

    private void SetupUpgradeView()
    {
        chooseRoomButton.gameObject.SetActive(false);
        titleText.text = $"{activeSlot.CurrentRoomType} (Lvl {activeSlot.CurrentLevel})";

        RoomDefinition def = activeSlot.CurrentDefinition;

        if (def == null)
        {
            Debug.LogError("Room definition missing");
            return;
        }

        if (activeSlot.CurrentLevel >= def.levels.Length)
        {
            costText.text = "Max Level Reached";
            actionButton.interactable = false;
            return;
        }

        var costs = def.levels[activeSlot.CurrentLevel].costs;
        UpdateCostUIAndButton(costs, false);
    }

    private void UpdateCostUIAndButton(List<ResourceCost> costs, bool isBuilding)
    {
        string costString = isBuilding ? "Cost for building:\n" : "Cost for next level:\n";
        bool canAfford = true;

        foreach (var cost in costs)
        {
            int currentAmount = ResourceManager.Instance.GetResourceCount(cost.resourceName);
            costString += $"{cost.resourceName}: {cost.amount} (You have: {currentAmount})\n";

            if (currentAmount < cost.amount)
                canAfford = false;
        }

        costText.text = costString;
        actionButton.interactable = canAfford;
    }

    private void OnActionButtonClicked()
    {
        RoomDefinition def = activeSlot.CurrentRoomType == RoomType.None ? selectedPrototype : activeSlot.CurrentDefinition;
        int levelIndex = activeSlot.CurrentRoomType == RoomType.None ? 0 : activeSlot.CurrentLevel;

        if (def == null)
        {
            Debug.LogError("Cannot build or upgrade, definition is null");
            return;
        }

        var costs = def.levels[levelIndex].costs;

        foreach (var cost in costs)
            ResourceManager.Instance.ConsumeResource(cost.resourceName, cost.amount);

        activeSlot.BuildOrUpgradeRoom(def);
        RefreshUI();
    }
}