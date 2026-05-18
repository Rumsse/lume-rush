using UnityEngine;
using UnityEngine.UI;
using Visualization;

[RequireComponent(typeof(Button))]
public class UIRoomSlot : MonoBehaviour
{
    [SerializeField] private RoomSlot target3DSlot;
    [SerializeField] private RoomUpgradesManager uiManager;

    private Button button;

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable() => button.onClick.AddListener(OnClicked);

    private void OnDisable() => button.onClick.RemoveListener(OnClicked);

    private void OnClicked()
    {
        if (target3DSlot == null)
        {
            Debug.LogError("Target 3D RoomSlot missing");
            return;
        }

        uiManager.OpenRoomDetails(target3DSlot);
    }
}