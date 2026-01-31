using UnityEngine;

public class SlotController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public SlotDataSO stationData;

    [Header("State")]
    public bool IsSelected { get; private set; }

    public bool IsEmpty => stationData == null;

    public SlotDataSO StationData => stationData;

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
    }
    public void BuildStation(SlotDataSO data)
    {
        if (data == null)
        {
            Debug.LogWarning($"Trying to build NULL station on {name}");
            return;
        }

        if (!IsEmpty)
        {
            Debug.LogWarning($"Slot {name} already has a station");
            return;
        }

        stationData = data;

        Debug.Log($"Built station: {data.stationName} on {name}");
    }

    public void ClearSlot()
    {
        stationData = null;
        Debug.Log($"Cleared slot {name}");
    }
}