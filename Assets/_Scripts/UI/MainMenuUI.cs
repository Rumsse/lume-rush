using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    #region Settings

    [SerializeField] EventReference hoverOverSound;
    [SerializeField] EventReference clickSound;
    [SerializeField] CelestialBodySettings startingPlanetPreset;

    #endregion

    #region UI Callbacks

    public void OnPointerEnter(PointerEventData eventData) => RuntimeManager.PlayOneShot(hoverOverSound);

    public void OnClickSFX() => RuntimeManager.PlayOneShot(clickSound);

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
            Debug.LogWarning("Starting Planet Preset is not assigned in MainMenuUI!");

        if (!SceneLoader.Instance)
            return;

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