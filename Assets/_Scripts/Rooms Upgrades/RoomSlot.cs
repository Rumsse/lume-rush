using UnityEngine;

public class RoomSlot : MonoBehaviour
{
    public RoomType CurrentRoomType => currentRoomType;
    public int CurrentLevel => currentLevel;
    public RoomDefinition CurrentDefinition => currentDefinition;

    [SerializeField] private RoomType currentRoomType = RoomType.None;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private RoomDefinition currentDefinition;

    private GameObject currentRoomInstance;

    public void BuildOrUpgradeRoom(RoomDefinition definition)
    {
        if (definition == null)
            return;

        currentDefinition = definition;

        if (currentRoomType == RoomType.None)
            currentLevel = 1;
        else if (currentLevel < currentDefinition.levels.Length)
            currentLevel++;
        else
        {
            Debug.LogWarning("Room already at max level");
            return;
        }

        currentRoomType = definition.roomType;
        SpawnRoomModel();
    }

    private void SpawnRoomModel()
    {
        ClearOldModels();

        var levelData = currentDefinition.levels[currentLevel - 1];

        if (levelData.roomPrefab == null)
        {
            Debug.LogError("Prefab missing");
            return;
        }

        currentRoomInstance = Instantiate(levelData.roomPrefab, transform);
        currentRoomInstance.transform.localPosition = levelData.spawnOffset;
        currentRoomInstance.transform.localEulerAngles = levelData.spawnRotation;
    }

    private void ClearOldModels()
    {
        if (currentRoomInstance != null)
            Destroy(currentRoomInstance);

        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
}