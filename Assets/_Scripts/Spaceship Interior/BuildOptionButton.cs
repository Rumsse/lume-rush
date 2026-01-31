using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildOptionButton : MonoBehaviour
{
    [Header("Station")]
    [SerializeField] private SlotDataSO stationData;

    [Header("Costs")]
    [SerializeField] private int titaniumCost;
    [SerializeField] private int aluminiumCost;
    [SerializeField] private int ironCost;
    [SerializeField] private int coalCost;

    private Button button;
    private BuildMenuController menu;

    public void Refresh(BuildMenuController menuController)
    {
        if (button == null)
            button = GetComponent<Button>();

        menu = menuController;

        if (menu == null || stationData == null)
        {
            button.interactable = false;
            return;
        }

        button.interactable = menu.CanAfford(
            titaniumCost,
            aluminiumCost,
            ironCost,
            coalCost
        );

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (menu == null || !menu.IsOpen)
            return;

        menu.Build(
            stationData,
            titaniumCost,
            aluminiumCost,
            ironCost,
            coalCost
        );
    }
}