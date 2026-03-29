using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetInteractable : BaseInteractable
{
    public Transform spawnPanelPosition;
    public string PlanetName;
    public string SelectedPresetName;

    GameObject spawnedPanel;
    Vector3 baseScale;

    public SFX_SimpleSoundInteract sfx;

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
        //sfx.PlaySound_UIClick();
        if (spawnedPanel) return;

        var globalPanel = GameObject.FindFirstObjectByType<PlanetConfirmationUI>(FindObjectsInactive.Include);
        if (!globalPanel) return;

        spawnedPanel = globalPanel.gameObject;
        spawnedPanel.SetActive(true);

        var displayName = string.IsNullOrEmpty(PlanetName) ? name : PlanetName;
        globalPanel.Setup(displayName, OnConfirmAction, OnCancelAction);
    }

    void OnConfirmAction()
    {
        //sfx.PlaySound_UIClick();
        var displayName = string.IsNullOrEmpty(PlanetName) ? name : PlanetName;

        PlayerPrefs.SetString("SelectedPlanet", displayName);
        PlayerPrefs.SetString("SelectedPlanetPreset", string.IsNullOrEmpty(SelectedPresetName) ? displayName : SelectedPresetName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Test Planet Exploration");
    }

    void OnCancelAction()
    {
        //sfx.PlaySound_UIClick();
        if (spawnedPanel) spawnedPanel.SetActive(false);
        spawnedPanel = null;
    }
}