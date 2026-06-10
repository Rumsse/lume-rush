using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    Dictionary<string, int> globalResources = new Dictionary<string, int>();

    [SerializeField] int startingFuel = 500;

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

    void Start() => AddResource("Fuel", startingFuel);

    public void AddResource(string resourceName, int amount)
    {
        if (amount <= 0)
            return;

        if (globalResources.ContainsKey(resourceName))
            globalResources[resourceName] += amount;
        else
            globalResources[resourceName] = amount;

        Debug.Log($"[ResourceManager] Added {amount}x {resourceName}. Total: {globalResources[resourceName]}");
        ResourceUI.Instance?.UpdateUI();
    }

    public void AddResourceWithCategory(ResourceData resourceData, int amount)
    {
        if (resourceData == null || amount <= 0)
            return;

        string storageName = resourceData.GetStorageName();
        AddResource(storageName, amount);
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddResource("Aluminium", 100);
            AddResource("Titanium", 100);
            AddResource("Iron", 100);
            AddResource("Coal", 100);
            AddResource("Fuel", 100);

            Debug.Log("Added 100 of every resource");
        }
    }
}