using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarSlot
{
    public VisualElement slotElement;
    public string currentResourceName;
    public Sprite currentResourceIcon;

    public ToolbarSlot(VisualElement element)
    {
        slotElement = element;
        currentResourceName = null;
        currentResourceIcon = null;
    }

    public void SetItem(string resourceName, Sprite resourceIcon)
    {
        currentResourceName = resourceName;
        currentResourceIcon = resourceIcon;

        var icon = slotElement.Q<VisualElement>("Icon");

        if (resourceIcon != null)
        {
            icon.style.backgroundImage = new StyleBackground(resourceIcon.texture);
            icon.visible = true;
        }
        else
        {
            icon.visible = false;
        }
    }

    public void Clear()
    {
        currentResourceName = null;
        currentResourceIcon = null;

        var icon = slotElement.Q<VisualElement>("Icon");
        icon.style.backgroundImage = null;
        icon.visible = false;
    }

    public bool IsEmpty()
    {
        return currentResourceName == null;
    }
}
