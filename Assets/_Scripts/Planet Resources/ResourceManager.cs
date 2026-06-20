using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    #region Variables

    public static ResourceManager Instance { get; private set; }

    public int Fuel => globalResources.TryGetValue("Fuel", out var v) ? v : 0;

    [SerializeField] int startingFuel = 500;
    [SerializeField] string[] developerCheatResources = { "Titanium", "Aluminium", "Iron", "Coal", "Food", "Fuel" };

    Dictionary<string, int> globalResources = new Dictionary<string, int>();

    #endregion

    #region Unity Methods

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

    void Start() => ResetResources();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            GiveDeveloperResources();
    }

    #endregion

    #region Resource Management

    public void AddResource(string resourceName, int amount)
    {
        if (amount <= 0)
            return;

        if (globalResources.ContainsKey(resourceName))
            globalResources[resourceName] += amount;
        else
            globalResources[resourceName] = amount;

        Debug.Log($"[ResourceManager] Added {amount}x {resourceName}. Total: {globalResources[resourceName]}");

        if (ResourceUI.Instance)
            ResourceUI.Instance.UpdateUI();
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

        if (ResourceUI.Instance)
            ResourceUI.Instance.UpdateUI();

        return true;
    }

    public void ResetResources()
    {
        globalResources.Clear();
        AddResource("Fuel", startingFuel);
    }

    #endregion

    #region Developer Tools

    public void DebugInventory()
    {
        Debug.Log("=== Global Resources ===");
        foreach (var resource in globalResources)
            Debug.Log($"{resource.Key}: {resource.Value}");
    }

    void GiveDeveloperResources()
    {
        foreach (string resource in developerCheatResources)
            AddResource(resource, 20);

        Debug.Log("[ResourceManager] Developer cheat applied: +20 all resources");
    }

    #endregion
}