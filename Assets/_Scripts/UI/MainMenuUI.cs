using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] EventReference hoverOverSound;
    [SerializeField] EventReference clickSound;
    [SerializeField] EventReference pauseMenuClose;
    [SerializeField] EventReference pauseMenuOpen;
    [SerializeField] CelestialBodySettings startingPlanetPreset;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null)
            return;

        if (!button.interactable)
            return;

        RuntimeManager.PlayOneShot(hoverOverSound);
    }

    public void OnClickSFX()
    {
        RuntimeManager.PlayOneShot(clickSound);
    }

    public void OnPlayButton()
    {
        OnClickSFX();

        PlanetSelectionManager.ResetMapData();
        ShipSaveManager.ResetShipData();

        if (startingPlanetPreset != null)
        {
            PlayerPrefs.SetString("SelectedPlanetPreset", startingPlanetPreset.name);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Starting Planet Preset is not assigned in MainMenuUI!");
        }

        if (!SceneLoader.Instance)
            return;

        SceneLoader.Instance.LoadPlanet();
    }

    public void PauseMenuClose()
    {
        RuntimeManager.PlayOneShot(pauseMenuClose);
    }

    public void PauseMenuOpen()
    {
        RuntimeManager.PlayOneShot(pauseMenuOpen);
    }

    public void OnContinueButton()
    {
        OnClickSFX();

        if (!SceneLoader.Instance)
            return;

        SceneLoader.Instance.LoadMap();
    }

    public void OnSettingsButton()
    {
        OnClickSFX();
        Debug.Log("Settings clicked");
    }

    public void OnQuitButton()
    {
        OnClickSFX();

        if (!SceneLoader.Instance)
            return;

        SceneLoader.Instance.QuitGame();
    }
}