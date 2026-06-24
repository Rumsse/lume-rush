using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Timing")]
    [SerializeField] private float delayBeforeMainMenu = 5f;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        yield return new WaitForSeconds(delayBeforeMainMenu);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}