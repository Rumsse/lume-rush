using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] EventReference hoverOverSound;
    [SerializeField] EventReference clickSound;

    public void OnPointerEnter(PointerEventData eventData) => RuntimeManager.PlayOneShot(hoverOverSound);

    public void OnClickSFX() => RuntimeManager.PlayOneShot(clickSound);

    public void OnPlayButton()
    {
        OnClickSFX();
        PlanetSelectionManager.ResetMapData();

        if (!SceneLoader.Instance)
            return;

        SceneLoader.Instance.LoadMap();
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