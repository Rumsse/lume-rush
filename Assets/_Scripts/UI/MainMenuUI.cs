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
    [SerializeField] IntroCinematicController cinematicController;

    Button button;

    #region Unity Callbacks

    void Awake() => button = GetComponent<Button>();

    #endregion

    #region Audio

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button || !button.interactable)
            return;

        RuntimeManager.PlayOneShot(hoverOverSound);
    }

    public void OnClickSFX() => RuntimeManager.PlayOneShot(clickSound);

    public void PauseMenuClose() => RuntimeManager.PlayOneShot(pauseMenuClose);

    public void PauseMenuOpen() => RuntimeManager.PlayOneShot(pauseMenuOpen);

    #endregion

    #region UI Actions

    public void OnPlayButton()
    {
        OnClickSFX();

        PlanetSelectionManager.ResetMapData();
        ShipSaveManager.ResetShipData();

        PlayerPrefs.SetInt("TutorialDone", 0);

        if (startingPlanetPreset)
        {
            PlayerPrefs.SetString("SelectedPlanetPreset", startingPlanetPreset.name);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Starting Planet Preset is not assigned in MainMenuUI!");
        }

        if (cinematicController)
            cinematicController.StartCinematic();
        else if (SceneLoader.Instance)
            SceneLoader.Instance.LoadPlanet();
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

    #endregion
}