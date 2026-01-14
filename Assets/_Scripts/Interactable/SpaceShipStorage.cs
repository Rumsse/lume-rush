using System.Collections.Generic;
using UnityEngine;

public class SpaceShipStorage : MonoBehaviour
{
    Dictionary<string, int> storedResources = new Dictionary<string, int>();

    public int Fuel => storedResources.TryGetValue("Fuel", out var v) ? v : 0;

    public void AddToStorage(List<ToolbarSlot> toolbarSlots)
    {
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            var slot = toolbarSlots[i];
            if (slot == null || slot.IsEmpty()) continue;

            var resourceName = slot.currentResourceName;
            if (string.IsNullOrEmpty(resourceName)) { slot.Clear(); continue; }

            if (storedResources.ContainsKey(resourceName)) storedResources[resourceName]++;
            else storedResources[resourceName] = 1;

            Debug.Log($"Added {resourceName} to the ship.");
            slot.Clear();
        }

        Debug.Log("Toolbar cleared and resources added to the ship.");
    }

    public void AddFuel(int amount)
    {
        if (amount <= 0) return;
        if (storedResources.ContainsKey("Fuel")) storedResources["Fuel"] += amount;
        else storedResources["Fuel"] = amount;
        Debug.Log($"Added Fuel: {amount}. Total Fuel: {storedResources["Fuel"]}");
    }

    public int GetResourcesCount(string resourceName) => storedResources.TryGetValue(resourceName, out var count) ? count : 0;

    public void DebugShipInventory()
    {
        Debug.Log("Current ship inventory:");
        foreach (var resource in storedResources) Debug.Log($"{resource.Key}: {resource.Value}");
    }
}