using UnityEngine;

public class RoomUpgradesManager : MonoBehaviour
{
    [SerializeField] private GameObject shipGridPanel;
    [SerializeField] private RoomDetailsPanel roomDetailsPanel;
    [SerializeField] private GameObject roomSelectionPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleShipGrid();
    }

    public void ToggleShipGrid()
    {
        bool state = !shipGridPanel.activeSelf;
        shipGridPanel.SetActive(state);

        if (!state)
            roomDetailsPanel.gameObject.SetActive(false);
    }

    public void OpenRoomDetails(RoomSlot slot) => roomDetailsPanel.ShowPanel(slot);

    public void CloseRoomDetails() => roomDetailsPanel.gameObject.SetActive(false);

    public void CloseRoomSelection() => roomSelectionPanel.gameObject.SetActive(false);

    public void CloseShipGridPanel() => shipGridPanel.SetActive(false);
}