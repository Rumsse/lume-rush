using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    Dictionary<string, int> globalResources = new Dictionary<string, int>();

    public int Fuel => globalResources.TryGetValue("Fuel", out var v) ? v : 0;

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

    public void AddResource(string resourceName, int amount)
    {
        if (amount <= 0)
            return;

        if (globalResources.ContainsKey(resourceName))
            globalResources[resourceName] += amount;
        else
            globalResources[resourceName] = amount;

        ResourceUI.Instance?.UpdateUI();
    }

    public void AddResources(Dictionary<string, int> resources)
    {
        foreach (var resource in resources)
            AddResource(resource.Key, resource.Value);
    }

    public int GetResourceCount(string resourceName) => globalResources.TryGetValue(resourceName, out var count) ? count : 0;

    public Dictionary<string, int> GetAllResources() => new Dictionary<string, int>(globalResources);

    public bool ConsumeResource(string resourceName, int amount)
    {
        if (!globalResources.ContainsKey(resourceName) || globalResources[resourceName] < amount)
            return false;

        globalResources[resourceName] -= amount;
        ResourceUI.Instance?.UpdateUI();
        return true;
    }

    public void DebugInventory()
    {
        Debug.Log("=== Global Resources ===");
        foreach (var resource in globalResources)
            Debug.Log($"{resource.Key}: {resource.Value}");
    }
}