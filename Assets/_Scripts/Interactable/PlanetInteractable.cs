using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetInteractable : BaseInteractable
{
    public Transform spawnPanelPosition;
    public CelestialBodySettings planetSettings;
    public SFX_SimpleSoundInteract sfx;
    [SerializeField] int currentFuelCost = 50;

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
        //transform.localScale = baseScale;
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

        spawnedPanel = globalPanel.gameObject;
        spawnedPanel.SetActive(true);

        globalPanel.Setup(planetSettings, currentFuelCost, OnConfirmAction, OnCancelAction);
    }

    void OnConfirmAction()
    {
        //sfx.PlaySound_UIClick();

        var displayName = string.IsNullOrEmpty(planetSettings.planetName) ? name : planetSettings.planetName;

        PlayerPrefs.SetString("SelectedPlanet", displayName);
        PlayerPrefs.SetString("SelectedPlanetPreset", planetSettings.name);
        PlayerPrefs.Save();

        SceneManager.LoadScene("_Planet_Exploration");
    }

    void OnCancelAction()
    {
        //sfx.PlaySound_UIClick();

        if (spawnedPanel)
            spawnedPanel.SetActive(false);

        spawnedPanel = null;
    }
}