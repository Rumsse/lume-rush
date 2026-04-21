using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetInteractable : BaseInteractable
{
    public Transform spawnPanelPosition;
    public CelestialBodySettings planetSettings;
    public SFX_SimpleSoundInteract sfx;

    [Header("Fuel Settings")]
    [SerializeField] int baseFuelCost = 10;
    [SerializeField] float costPerUnitDistance = 0.5f;

    GameObject spawnedPanel;
    Vector3 baseScale;

    void Awake() => baseScale = transform.localScale;

    protected override void OnMouseEnter()
    {
        //sfx.PlaySound_UIHover();
        transform.localScale = baseScale * 1.2f;
    }

    protected override void OnMouseExit()
    {
        transform.localScale = baseScale;
    }

    protected override void OnMouseDown()
    {
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

    int CalculateTravelCost()
    {
        float distance = Vector3.Distance(transform.position, PlanetSelectionManager.CurrentPlayerPosition);

        if (distance < 0.1f)
            return 0;

        return baseFuelCost + Mathf.RoundToInt(distance * costPerUnitDistance);
    }

    void OnConfirmAction()
    {
        var displayName = string.IsNullOrEmpty(planetSettings.planetName) ? name : planetSettings.planetName;

        PlanetSelectionManager.UpdatePlayerPosition(transform.position);

        PlayerPrefs.SetString("SelectedPlanet", displayName);
        PlayerPrefs.SetString("SelectedPlanetPreset", planetSettings.name);
        PlayerPrefs.Save();

        SceneManager.LoadScene("_Planet_Exploration");
    }

    void OnCancelAction()
    {
        if (spawnedPanel)
            spawnedPanel.SetActive(false);

        spawnedPanel = null;
    }
}