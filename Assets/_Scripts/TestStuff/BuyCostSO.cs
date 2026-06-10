using UnityEngine;

[CreateAssetMenu(fileName = "BuyCostSO", menuName = "Workshop/BuyCostSO")]
public class BuyCostSO : ScriptableObject
{
    public BuyCostItem[] items;
}

[System.Serializable]
public class BuyCostItem
{
    public ResourceData resource;
    public int resourceAmount;
}