using UnityEngine;
using FMODUnity;

public class PauseManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private EventReference pauseMenuOpen;

    #endregion

    #region Private Fields

    private bool isPaused;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    #endregion

    #region Pause Logic

    public void TogglePause()
    {
        RuntimeManager.PlayOneShot(pauseMenuOpen);
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void OnResumeButton() => TogglePause();

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadMainMenu();
    }

    public void OnQuitButton()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.QuitGame();
    }

    #endregion
}