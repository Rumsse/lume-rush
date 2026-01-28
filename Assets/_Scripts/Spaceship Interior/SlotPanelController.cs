using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanelController : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public GameObject panelUI;
    public StatsDisplay SD;
    public Transform statParent;

    public string stationName;
    public int stationLevel;
    public float stationBaseSpeed;
    public float stationBaseSlotAmount;
    public float stationBaseSlotCapacity;
    public float stationBaseMaterialSaveChance;
    public float stationBaseCapacity;

    public void PanelUIOff()
    {
        panelUI.SetActive(false);
        foreach(TextMeshProUGUI stat in SD.statList)
        {
            Destroy(stat.gameObject);
        }
        SD.statList.Clear();

    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        PanelUIOff();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        SD.UpdateStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
