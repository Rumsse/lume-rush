using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class WorkshopWinChoiceUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject workshopUI;
    [SerializeField] private GameObject choicePanel;

    [Header("Scenes")]
    [SerializeField] private string winSceneName = "WinScene";

    [Header("SFX")]
    [SerializeField] private EventReference yesSound;
    [SerializeField] private EventReference noSound;

    public void OnNoButton()
    {
        PlaySound(noSound);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (workshopUI != null)
            workshopUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnYesButton()
    {
        PlaySound(yesSound);

        Time.timeScale = 1f;
        SceneManager.LoadScene(winSceneName);
    }

    private void PlaySound(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound);
    }
}