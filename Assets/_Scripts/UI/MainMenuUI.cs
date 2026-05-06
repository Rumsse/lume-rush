using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private EventReference hoverOverSound;
    [SerializeField] private EventReference clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(hoverOverSound);
    }

    public void OnClickSFX()
    {
        RuntimeManager.PlayOneShot(clickSound);
    }

    public void OnPlayButton()
    {
        OnClickSFX();
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadMap();
    }

    public void OnContinueButton()
    {
        OnClickSFX();
        if (SceneLoader.Instance != null)
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
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.QuitGame();
    }
}