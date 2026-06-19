using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject gameOverPanel;

    void Awake() => HideGameOver();

    void OnEnable() => PlayerHealthSystem.OnPlayerDeath += ShowGameOver;

    void OnDisable() => PlayerHealthSystem.OnPlayerDeath -= ShowGameOver;

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
}