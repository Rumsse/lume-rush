using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarManager : MonoBehaviour
{
    public static ToolbarManager Instance { get; private set; }

    public UIDocument uiDocument;
    private List<ToolbarSlot> slots = new List<ToolbarSlot>();

    void Awake()
    {
        Instance = this;

        var root = uiDocument.rootVisualElement;
        var slotElements = root.Query<VisualElement>("Slot").ToList();

        foreach (var elem in slotElements)
        {
            slots.Add(new ToolbarSlot(elem));
        }
    }

    public bool AddItem(string itemName, Sprite itemIcon)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(itemName, itemIcon);
                Debug.Log($"Dodano przedmiot {itemName} do Toolbara.");
                return true;
            }
        }
        Debug.LogWarning("Brak wolnego slotu w toolbarze!");
        return false;
    }

    public List<ToolbarSlot> GetSlots()
    {
        return slots;
    }

    public void ClearToolbar()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
    }
}
