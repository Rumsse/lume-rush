using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class PlanetInteractable : BaseInteractable
{
    #region Settings

    public Transform spawnPanelPosition;
    public CelestialBodySettings planetSettings;
    public SFX_SimpleSoundInteract sfx;

    [Header("SFX")]
    [SerializeField]
    private EventReference hoverSound =
        EventReference.Find("event:/UI/UI_Planet_Hover");

    [SerializeField]
    private EventReference clickSound =
        EventReference.Find("event:/UI/UI_Planet_MenuOpen");

    [Header("Fuel Settings")]
    [SerializeField] int baseFuelCost = 10;
    [SerializeField] float costPerUnitDistance = 0.5f;

    #endregion

    #region State

    GameObject spawnedPanel;
    Vector3 baseScale;
    bool hasBurned;

    bool IsBurned => Vector3.Distance(Vector3.zero, transform.position) <= SupernovaManager.Instance.CurrentRadius;

    #endregion

    #region Unity Callbacks

    void Awake() => baseScale = transform.localScale;

    void Start()
    {
        if (IsBurned)
            ApplyBurnedState();
    }

    void Update()
    {
        if (hasBurned)
            return;

        if (IsBurned)
            ApplyBurnedState();
    }

    protected override void OnMouseEnter()
    {
        if (hasBurned)
            return;

        PlaySound(hoverSound);

        transform.localScale = baseScale * 1.2f;
    }

    protected override void OnMouseExit()
    {
        transform.localScale = baseScale;
    }

    protected override void OnMouseDown()
    {
        if (hasBurned)
        {
            Debug.Log("Planet is already consumed by the supernova.");
            return;
        }

        PlaySound(clickSound);

        if (spawnedPanel)
            return;

        var globalPanel = GameObject.FindFirstObjectByType<PlanetConfirmationUI>(FindObjectsInactive.Include);
        if (!globalPanel)
        {
            Debug.LogError("PlanetConfirmationUI not found in scene");
            return;
        }

        int calculatedCost = CalculateTravelCost();

        spawnedPanel = globalPanel.gameObject;
        spawnedPanel.SetActive(true);

        globalPanel.Setup(planetSettings, calculatedCost, OnConfirmAction, OnCancelAction);
    }

    #endregion

    #region Logic

    void ApplyBurnedState()
    {
        hasBurned = true;

        if (spawnedPanel)
            OnCancelAction();

        if (planetSettings.burnedPrefab)
        {
            var burnedPlanet = Instantiate(
                planetSettings.burnedPrefab,
                transform.position,
                transform.rotation,
                transform.parent
            );

            burnedPlanet.name = gameObject.name;
        }

        Destroy(gameObject);
    }

    int CalculateTravelCost()
    {
        float distance = Vector3.Distance(transform.position, PlanetSelectionManager.CurrentPlayerPosition);

        if (distance < 0.1f)
            return 0;

        return baseFuelCost + Mathf.RoundToInt(distance * costPerUnitDistance);
    }

    void OnConfirmAction()
    {
        var displayName = string.IsNullOrEmpty(planetSettings.planetName)
            ? name
            : planetSettings.planetName;

        PlanetSelectionManager.UpdatePlayerPosition(transform.position);

        PlayerPrefs.SetString("SelectedPlanet", displayName);
        PlayerPrefs.SetString("SelectedPlanetPreset", planetSettings.name);

        SupernovaManager.Instance.SaveRadius();
        PlayerPrefs.Save();

        SceneManager.LoadScene("_Planet_Exploration");
    }

    void OnCancelAction()
    {
        if (!spawnedPanel)
            return;

        spawnedPanel.SetActive(false);
        spawnedPanel = null;
    }

    void PlaySound(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound, transform.position);
    }

    #endregion
}