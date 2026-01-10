using System.Collections.Generic;
using UnityEngine;

public class SpaceShipStorage : MonoBehaviour
{
    private Dictionary<string, int> storedItems = new Dictionary<string, int>();


    public void AddToStorage(List<ToolbarSlot> toolbarSlots)
    {
        foreach (var slot in toolbarSlots)
        {
            if (!slot.IsEmpty())
            {
                string itemName = slot.currentItemName;

                if (storedItems.ContainsKey(itemName))
                {
                    storedItems[itemName]++;
                }
                else
                {
                    storedItems[itemName] = 1;
                }

                Debug.Log($"Added {itemName} to the ship.");
                slot.Clear();
            }
        }

        Debug.Log("Toolbar cleared and items added to the ship.");
    }

    public void DebugShipInventory()
    {
        Debug.Log("Current ship inventory:");

        foreach (var item in storedItems)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
    }
}