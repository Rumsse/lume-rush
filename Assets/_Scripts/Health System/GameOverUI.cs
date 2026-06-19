using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    #region Settings

    [SerializeField] GameObject gameOverPanel;

    #endregion

    #region Unity Callbacks

    void Awake() => HideGameOver();

    void OnEnable()
    {
        PlayerHealthSystem.OnPlayerDeath += ShowGameOver;
        SupernovaManager.OnSupernovaHitPlayer += ShowGameOver;
    }

    void OnDisable()
    {
        PlayerHealthSystem.OnPlayerDeath -= ShowGameOver;
        SupernovaManager.OnSupernovaHitPlayer -= ShowGameOver;
    }

    #endregion

    #region Logic

    void ShowGameOver()
    {
        if (gameOverPanel == null)
            return;

        gameOverPanel.SetActive(true);
    }

    void HideGameOver()
    {
        if (gameOverPanel == null)
            return;

        gameOverPanel.SetActive(false);
    }

    #endregion
}