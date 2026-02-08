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

            var resourceData = slot.currentResourceData;
            if (resourceData == null)
            {
                slot.Clear();
                continue;
            }

            ResourceManager.Instance?.AddResourceWithCategory(resourceData, 1);
            Debug.Log($"Added {resourceData.resourceName} → {resourceData.GetStorageName()} to storage.");
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