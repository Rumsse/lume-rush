using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MainMenuUI : MonoBehaviour
{

    public void OnPlayButton()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadShip();
    }

    public void OnContinueButton()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadMap();
    }

    public void OnSettingsButton()
    {
        Debug.Log("Settings clicked");
    }

    public void OnQuitButton()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.QuitGame();
    }
}