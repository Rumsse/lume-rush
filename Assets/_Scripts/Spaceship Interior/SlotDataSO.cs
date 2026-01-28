using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Scriptable Objects/Station Data")]
public class SlotDataSO : ScriptableObject
{
    [SerializeField] public string stationName;
    [SerializeField] public int stationLevel;

    #region Variables
    //Shared if more than 1 station uses it
    [HideInInspector] public float stationBaseSpeed;
    [HideInInspector] public float stationBaseSlotAmount;
    [HideInInspector] public float stationBaseSlotCapacity;

    //Station Specific
    //Workshop
    //[HideInInspector] public float stationBaseSpeed;
    //[HideInInspector] public float stationBaseSlotAmount;
    //[HideInInspector] public float stationBaseSlotCapacity;
    [HideInInspector] public float stationBaseMaterialSaveChance;

    //Kitchen
    //[HideInInspector] public float stationBaseSpeed;
    //[HideInInspector] public float stationBaseSlotAmount;
    //[HideInInspector] public float stationBaseSlotCapacity;

    //Cabin
    [HideInInspector][SerializeField] public float stationBaseCapacity;

    //Storage
    //[HideInInspector] public float stationBaseSlotAmount;
    //[HideInInspector] public float stationBaseSlotCapacity;

    #endregion
}
