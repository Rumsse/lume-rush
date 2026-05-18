using UnityEngine;

public class RoomUpgradesManager : MonoBehaviour
{
    [SerializeField] private GameObject shipGridPanel;
    [SerializeField] private RoomDetailsPanel roomDetailsPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleShipGrid();
    }

    private void ToggleShipGrid()
    {
        bool state = !shipGridPanel.activeSelf;
        shipGridPanel.SetActive(state);

        if (!state)
            roomDetailsPanel.gameObject.SetActive(false);
    }

    public void OpenRoomDetails(RoomSlot slot) => roomDetailsPanel.ShowPanel(slot);
}