using System.Collections.Generic;
using UnityEngine;

public class SpaceShipStorage : MonoBehaviour
{
    Dictionary<string, int> storedItems = new Dictionary<string, int>();

    public int Fuel => storedItems.TryGetValue("Fuel", out var v) ? v : 0;

    public void AddToStorage(List<ToolbarSlot> toolbarSlots)
    {
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            var slot = toolbarSlots[i];
            if (slot == null || slot.IsEmpty()) continue;

            var itemName = slot.currentItemName;
            if (string.IsNullOrEmpty(itemName)) { slot.Clear(); continue; }

            if (storedItems.ContainsKey(itemName)) storedItems[itemName]++;
            else storedItems[itemName] = 1;

            Debug.Log($"Added {itemName} to the ship.");
            slot.Clear();
        }

        Debug.Log("Toolbar cleared and items added to the ship.");
    }

    public void AddFuel(int amount)
    {
        if (amount <= 0) return;
        if (storedItems.ContainsKey("Fuel")) storedItems["Fuel"] += amount;
        else storedItems["Fuel"] = amount;
        Debug.Log($"Added Fuel: {amount}. Total Fuel: {storedItems["Fuel"]}");
    }

    public int GetItemCount(string itemName) => storedItems.TryGetValue(itemName, out var count) ? count : 0;

    public void DebugShipInventory()
    {
        Debug.Log("Current ship inventory:");
        foreach (var item in storedItems) Debug.Log($"{item.Key}: {item.Value}");
    }
}