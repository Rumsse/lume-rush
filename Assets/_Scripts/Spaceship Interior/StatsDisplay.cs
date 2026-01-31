using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    public TextMeshProUGUI stationNameText;
    public TextMeshProUGUI statTextPrefab;
    public Transform statDisplayParent;


    public List<TextMeshProUGUI> statList = new();


    public void DisplayStats(SlotDataSO data)
    {
        Clear();


        stationNameText.text = data.stationName;


        AddStat("Station Level", data.stationLevel);
        AddStat("Speed", data.stationBaseSpeed);
        AddStat("Capacity", data.stationBaseCapacity);
        AddStat("Slot Amount", data.stationBaseSlotAmount);
        AddStat("Slot Capacity", data.stationBaseSlotCapacity);
        AddStat("Material Save Chance", data.stationBaseMaterialSaveChance, "%");

        //Debug.Log($"Speed = {data.stationBaseSpeed}");
    }


    void AddStat(string label, float value, string suffix = "")
    {
        if (value <= 0) return;


        var stat = Instantiate(statTextPrefab, statDisplayParent);
        stat.text = $"{label}: {value}{suffix}";
        statList.Add(stat);
    }


    void AddStat(string label, int value)
    {
        if (value <= 0) return;


        var stat = Instantiate(statTextPrefab, statDisplayParent);
        stat.text = $"{label}: {value}";
        statList.Add(stat);
    }


    public void Clear()
    {
        foreach (var stat in statList)
            Destroy(stat.gameObject);


        statList.Clear();
    }
}