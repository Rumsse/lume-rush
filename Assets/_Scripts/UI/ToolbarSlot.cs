using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarSlot
{
    public VisualElement slotElement;
    public string currentResourceName;
    public Sprite currentResourceIcon;
    public ResourceData currentResourceData;

    public ToolbarSlot(VisualElement element)
    {
        slotElement = element;
        currentResourceName = null;
        currentResourceIcon = null;
        currentResourceData = null;
    }

    public void SetItem(string resourceName, Sprite resourceIcon, ResourceData resourceData = null)
    {
        currentResourceName = resourceName;
        currentResourceIcon = resourceIcon;
        currentResourceData = resourceData;

        var icon = slotElement.Q<VisualElement>("Icon");

        if (resourceIcon != null)
        {
            icon.style.backgroundImage = new StyleBackground(resourceIcon.texture);
            icon.visible = true;
        }
        else
            icon.visible = false;
    }

    public void Clear()
    {
        currentResourceName = null;
        currentResourceIcon = null;
        currentResourceData = null;

        var icon = slotElement.Q<VisualElement>("Icon");
        icon.style.backgroundImage = null;
        icon.visible = false;
    }

    public bool IsEmpty() => currentResourceName == null;
}