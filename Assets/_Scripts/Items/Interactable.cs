using UnityEngine;

public class Interactable : MonoBehaviour
{
    public ItemData itemData;

    public string ItemName => itemData?.itemName;
    public Sprite ItemIcon => itemData?.itemIcon;
}
