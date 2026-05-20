using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    public static ResourceStorage Instance;

    public Dictionary<string, int> resources = new Dictionary<string, int>();

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        resources["Iron"] = 40;
        resources["Aluminium"] = 20;
        resources["Coal"] = 50;
        resources["Titanium"] = 20;
    }

    public bool HasEnough(string key, int amount)
    {
        return resources.TryGetValue(key, out int owned) && owned >= amount;
    }

    public void Spend(string key, int amount)
    {
        if (resources.TryGetValue(key, out int owned))
        {
            resources[key] = owned - amount;
        }
    }
}