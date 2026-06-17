using UnityEngine;
using FMODUnity;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private EventReference hoverOverSound;
    [SerializeField] private EventReference clickSound;
    [SerializeField] private EventReference pauseMenuClose;
    [SerializeField] private EventReference pauseMenuOpen;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button button = eventData.pointerEnter.GetComponentInParent<Button>();

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
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadMap();
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