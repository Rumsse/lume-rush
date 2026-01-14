using UnityEngine;

[CreateAssetMenu(fileName = "NewResource", menuName = "Resources/ResourceData")] 
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public GameObject prefab;
    public Sprite resourceIcon;
    
}
