using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelData", menuName = "Ship/Room Level Data")]
public class RoomLevelData : ScriptableObject
{
    public int level;
    public int cost;
    public GameObject roomPrefab;
    public Sprite previewImage;
}
