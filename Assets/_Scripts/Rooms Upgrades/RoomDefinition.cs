using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomDefinition", menuName = "Ship/Room Definition")]
public class RoomDefinition : ScriptableObject
{
    public RoomType roomType;
    public RoomLevelData[] levels;
}

public enum RoomType { None, Kitchen, Bedroom, Workshop, Storage }
