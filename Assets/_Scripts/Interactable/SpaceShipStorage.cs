using System.Collections.Generic;
using UnityEngine;

public class SpaceShipStorage : MonoBehaviour
{
    Dictionary<string, int> temporaryStorage = new Dictionary<string, int>();

    void Start()
    {
        if (ResourceManager.Instance != null)
            temporaryStorage = ResourceManager.Instance.GetAllResources();
    }

    public void ReceiveResources(List<ToolbarSlot> toolbarSlots)
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

            ResourceManager.Instance?.AddResource(resourceName, 1);
            Debug.Log($"Added {resourceName} to ship storage.");
            slot.Clear();
        }

        Debug.Log("Resources transferred to global storage.");
    }

    public void ReceiveFuel(int amount)
    {
        if (amount <= 0)
            return;

        ResourceManager.Instance?.AddResource("Fuel", amount);
        Debug.Log($"Added Fuel: {amount}");
    }

    public void DebugShipInventory()
    {
        ResourceManager.Instance?.DebugInventory();
    }
}