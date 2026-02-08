using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarManager : MonoBehaviour
{
    public static ToolbarManager Instance { get; private set; }

    public UIDocument uiDocument;
    List<ToolbarSlot> slots = new List<ToolbarSlot>();

    void Awake()
    {
        Instance = this;

        var root = uiDocument.rootVisualElement;
        var slotElements = root.Query<VisualElement>("Slot").ToList();

        foreach (var elem in slotElements)
            slots.Add(new ToolbarSlot(elem));
    }

    public bool AddResource(ResourceData resourceData)
    {
        if (resourceData == null)
            return false;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(resourceData.resourceName, resourceData.resourceIcon, resourceData);
                Debug.Log($"Resource added {resourceData.resourceName} to the Toolbar.");
                return true;
            }
        }

        Debug.LogWarning("No empty slot available in Toolbar!");
        return false;
    }

    public bool AddResource(string resourceName, Sprite resourceIcon)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(resourceName, resourceIcon, null);
                Debug.Log($"Resource added {resourceName} to the Toolbar.");
                return true;
            }
        }

        Debug.LogWarning("No empty slot available in Toolbar!");
        return false;
    }

    public List<ToolbarSlot> GetSlots() => slots;

    public void ClearToolbar()
    {
        foreach (var slot in slots)
            slot.Clear();
    }
}