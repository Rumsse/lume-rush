using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class StatsDisplay : MonoBehaviour
{
    public SlotPanelController SPC;
    public TextMeshProUGUI stationNameText;

    public TextMeshProUGUI statTextPrefab;
    private TextMeshProUGUI insStatText;

    public List<TextMeshProUGUI> statList;

    public Transform statDisplayParent;

    public string stationName;
    public int stationLevel;
    public float stationBaseSpeed;
    public float stationBaseSlotAmount;
    public float stationBaseSlotCapacity;
    public float stationBaseMaterialSaveChance;
    public float stationBaseCapacity;

    public void DisplayStats()
    {
        if (stationName != null)
        {
            stationNameText.text = stationName;

            insStatText = Instantiate(statTextPrefab);
            insStatText.text = "Station Level: " + stationLevel;
            insStatText.name = "StationLevel";
            insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
            statList.Add(insStatText);

            if (stationBaseSpeed > 0)
            {
                insStatText = Instantiate(statTextPrefab);
                insStatText.text = "Speed: " + stationBaseSpeed;
                insStatText.name = "BaseSpeed";
                insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
                statList.Add(insStatText);
            }

            if (stationBaseCapacity > 0)
            {
                insStatText = Instantiate(statTextPrefab);
                insStatText.text = "Capacity: " + stationBaseCapacity;
                insStatText.name = "BaseCapacity";
                insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
                statList.Add(insStatText);
            }

            if (stationBaseSlotAmount > 0)
            {
                insStatText = Instantiate(statTextPrefab);
                insStatText.text = "Slot Amount: " + stationBaseSlotAmount;
                insStatText.name = "BaseSlotAmount";
                insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
                statList.Add(insStatText);
            }

            if (stationBaseSlotCapacity > 0)
            {
                insStatText = Instantiate(statTextPrefab);
                insStatText.text = "Slot Capacity: " + stationBaseSlotCapacity;
                insStatText.name = "BaseSlotCapacity";
                insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
                statList.Add(insStatText);
            }

            if (stationBaseMaterialSaveChance > 0)
            {
                insStatText = Instantiate(statTextPrefab);
                insStatText.text = "Material Save Chance: " + stationBaseMaterialSaveChance + "%";
                insStatText.name = "BaseMaterialSaveChance";
                insStatText.GetComponent<Transform>().SetParent(statDisplayParent);
                statList.Add(insStatText);
            }   
        }
    }

    public void UpdateStats()
    {
        stationName = SPC.stationName;
        stationLevel = SPC.stationLevel;
        stationBaseSpeed = SPC.stationBaseSpeed;
        stationBaseSlotAmount = SPC.stationBaseSlotAmount;
        stationBaseSlotCapacity = SPC.stationBaseSlotCapacity;
        stationBaseMaterialSaveChance = SPC.stationBaseMaterialSaveChance;
        stationBaseCapacity = SPC.stationBaseCapacity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
