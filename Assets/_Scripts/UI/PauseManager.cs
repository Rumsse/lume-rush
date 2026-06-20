using UnityEngine;
using FMODUnity;

public class PauseManager : MonoBehaviour
{
    #region Serialized

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private EventReference pauseMenuOpen;

    #endregion

    #region State

    bool isPaused;
    bool isGameOver;

    #endregion

    #region Lifecycle

    void Awake()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void OnEnable() => PlayerHealthSystem.OnPlayerDeath += HandleGameOver;

    void OnDisable() => PlayerHealthSystem.OnPlayerDeath -= HandleGameOver;

    void Update()
    {
        if (isGameOver)
            return;

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

    void HandleGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    #region Buttons

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