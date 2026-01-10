using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/ItemData")] 
public class ItemData : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    public Sprite itemIcon;
    
}
