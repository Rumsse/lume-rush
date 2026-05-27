using System.Collections;
using UnityEngine;

public class RoomSlot : MonoBehaviour
{
    #region Properties

    public RoomType CurrentRoomType => currentRoomType;
    public int CurrentLevel => currentLevel;
    public RoomDefinition CurrentDefinition => currentDefinition;

    #endregion

    #region Variables

    [SerializeField] private RoomType currentRoomType = RoomType.None;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private RoomDefinition currentDefinition;

    [SerializeField] private Light roomLight;
    [SerializeField] private GameObject emptyRoomProps;
    [SerializeField] private float lightFadeDuration = 1.5f;

    private GameObject currentRoomInstance;
    private float targetLightIntensity;
    private Coroutine lightCoroutine;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (roomLight != null)
            targetLightIntensity = roomLight.intensity;
    }

    private void Start() => InitializeRoomState();

    #endregion

    #region Room Logic

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

        bool wasEmpty = currentRoomType == RoomType.None;
        currentRoomType = definition.roomType;
        SpawnRoomModel(wasEmpty);
    }

    private void InitializeRoomState()
    {
        if (roomLight == null)
            return;

        if (currentRoomType == RoomType.None)
        {
            roomLight.intensity = 0f;

            if (emptyRoomProps != null)
                emptyRoomProps.SetActive(true);
        }
        else
        {
            roomLight.intensity = targetLightIntensity;

            if (emptyRoomProps != null)
                emptyRoomProps.SetActive(false);
        }
    }

    private void SpawnRoomModel(bool wasEmpty)
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

        if (wasEmpty)
            TurnOnLight();
    }

    private void ClearOldModels()
    {
        if (currentRoomInstance != null)
            Destroy(currentRoomInstance);

        if (emptyRoomProps != null)
            emptyRoomProps.SetActive(false);
    }

    private void TurnOnLight()
    {
        if (roomLight == null)
            return;

        if (lightCoroutine != null)
            StopCoroutine(lightCoroutine);

        lightCoroutine = StartCoroutine(FadeLightRoutine());
    }

    private IEnumerator FadeLightRoutine()
    {
        float elapsedTime = 0f;
        roomLight.intensity = 0f;

        while (elapsedTime < lightFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            roomLight.intensity = Mathf.Lerp(0f, targetLightIntensity, elapsedTime / lightFadeDuration);
            yield return null;
        }

        roomLight.intensity = targetLightIntensity;
    }

    #endregion
}