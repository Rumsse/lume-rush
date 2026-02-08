using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Category", menuName = "Resources/Resource Category")]
public class ResourceCategory : ScriptableObject
{
    public string categoryName;
    public Sprite categoryIcon;
}