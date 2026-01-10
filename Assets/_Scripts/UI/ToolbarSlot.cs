using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarSlot
{
    public VisualElement slotElement;
    public string currentItemName;
    public Sprite currentItemIcon;

    public ToolbarSlot(VisualElement element)
    {
        slotElement = element;
        currentItemName = null;
        currentItemIcon = null;
    }

    public void SetItem(string itemName, Sprite itemIcon)
    {
        currentItemName = itemName;
        currentItemIcon = itemIcon;

        var icon = slotElement.Q<VisualElement>("Icon");

        if (itemIcon != null)
        {
            icon.style.backgroundImage = new StyleBackground(itemIcon.texture);
            icon.visible = true;
        }
        else
        {
            icon.visible = false;
        }
    }

    public bool IsEmpty()
    {
        return currentItemName == null;
    }
}
