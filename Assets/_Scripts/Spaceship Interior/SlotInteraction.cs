using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotInteraction : MonoBehaviour
{
    public Ray ray;
    public Camera mainCam;
    public SlotPanelController spc;

    public SlotDataSO cachedSlotDataSO;
    public GameObject cachedBuildSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void CastARay()
    {
        ray = new Ray(mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()), transform.forward);
        CheckForColliders();
        SlotPanelDataFill();
    }

    void CheckForColliders()
    {
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.tag == "BuildSlot")
        {
            cachedBuildSlot = hit.collider.gameObject;
            cachedBuildSlot.GetComponent<SlotController>()._selected = true;
            cachedBuildSlot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        else if (cachedBuildSlot != null)
        {
            cachedBuildSlot.GetComponent<SlotController>()._selected = false;
            cachedBuildSlot.transform.localScale = new Vector3(1f, 1f, 1f);
            cachedBuildSlot = null;
        }
    }

    void SlotPanelDataFill()
    {
        if(cachedBuildSlot != null)
        {
            cachedSlotDataSO = cachedBuildSlot.GetComponent<SlotController>().stationData;

            if(cachedSlotDataSO.stationName != null)
            {
                spc.stationName = cachedSlotDataSO.stationName;
            }

            spc.stationLevel = cachedSlotDataSO.stationLevel;
            spc.stationBaseSpeed = cachedSlotDataSO.stationBaseSpeed;
            spc.stationBaseSlotAmount = cachedSlotDataSO.stationBaseSlotAmount;
            spc.stationBaseSlotCapacity = cachedSlotDataSO.stationBaseSlotCapacity;
            spc.stationBaseMaterialSaveChance = cachedSlotDataSO.stationBaseMaterialSaveChance;
            spc.stationBaseCapacity = cachedSlotDataSO.stationBaseCapacity;
        }
        else
        {
            Debug.Log("No cached slot");
        }
    }


    

    // Update is called once per frame
    void Update()
    {
        CastARay();
    }
}

