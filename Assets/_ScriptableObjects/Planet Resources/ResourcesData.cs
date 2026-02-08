using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Resources/Resource Data")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite resourceIcon;
    public ResourceCategory category;

    public string GetStorageName() => category != null ? category.categoryName : resourceName;

    public Sprite GetStorageIcon() => category != null ? category.categoryIcon : resourceIcon;
}