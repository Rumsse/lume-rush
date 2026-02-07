using System.Collections.Generic;
using UnityEngine;

public class SpaceShipStorage : MonoBehaviour
{
    public static SpaceShipStorage Instance { get; private set; }

    Dictionary<string, int> storedResources = new Dictionary<string, int>();

    public int Fuel => storedResources.TryGetValue("Fuel", out var v) ? v : 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddToStorage(List<ToolbarSlot> toolbarSlots)
    {
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            var slot = toolbarSlots[i];
            if (slot == null || slot.IsEmpty())
                continue;

            var resourceName = slot.currentResourceName;
            if (string.IsNullOrEmpty(resourceName))
            {
                slot.Clear();
                continue;
            }

            AddResource(resourceName, 1);
            Debug.Log($"Added {resourceName} to the ship.");
            slot.Clear();
        }

        ResourceUI.Instance?.UpdateUI();
        Debug.Log("Toolbar cleared and resources added to the ship.");
    }

    public void AddResource(string resourceName, int amount)
    {
        if (amount <= 0)
            return;

        if (storedResources.ContainsKey(resourceName))
            storedResources[resourceName] += amount;
        else
            storedResources[resourceName] = amount;
    }

    public void AddFuel(int amount)
    {
        if (amount <= 0)
            return;

        AddResource("Fuel", amount);
        ResourceUI.Instance?.UpdateUI();
        Debug.Log($"Added Fuel: {amount}. Total Fuel: {Fuel}");
    }

    public int GetResourcesCount(string resourceName) => storedResources.TryGetValue(resourceName, out var count) ? count : 0;

    public Dictionary<string, int> GetAllResources() => new Dictionary<string, int>(storedResources);

    public void DebugShipInventory()
    {
        Debug.Log("Current ship inventory:");
        foreach (var resource in storedResources)
            Debug.Log($"{resource.Key}: {resource.Value}");
    }
}