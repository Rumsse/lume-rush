using UnityEngine;

public class PauseManager : MonoBehaviour
{
    #region Settings

    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;

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

    void OnEnable()
    {
        PlayerHealthSystem.OnPlayerDeath += HandleGameOver;
        SupernovaManager.OnSupernovaHitPlayer += HandleGameOver;
    }

    void OnDisable()
    {
        PlayerHealthSystem.OnPlayerDeath -= HandleGameOver;
        SupernovaManager.OnSupernovaHitPlayer -= HandleGameOver;
    }

    void Update()
    {
        if (isGameOver)
            return;

        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    #endregion

    #region Logic

    public void TogglePause()
    {
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

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

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