using UnityEngine;
using FMODUnity;

public class AudioMenuToggle : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject audioSettingsMenu;

    [Tooltip("Menu, z którego otwierane s¹ ustawienia audio")]
    [SerializeField] private GameObject parentMenu;

    [Header("SFX")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;

    [Header("Options")]
    [SerializeField] private bool unlockCursorWhenOpened = true;

    private bool audioMenuOpen;

    private void Start()
    {
        if (audioSettingsMenu != null)
            audioSettingsMenu.SetActive(false);

        audioMenuOpen = false;
    }

    private void Update()
    {
        if (audioMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            BackFromAudioSettings();
        }
    }

    public void OpenAudioSettings()
    {
        if (audioSettingsMenu == null)
            return;

        if (parentMenu != null)
            parentMenu.SetActive(false);

        audioSettingsMenu.SetActive(true);
        audioMenuOpen = true;

        PlaySound(openSound);

        if (unlockCursorWhenOpened)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void BackFromAudioSettings()
    {
        if (audioSettingsMenu == null)
            return;

        audioSettingsMenu.SetActive(false);
        audioMenuOpen = false;

        if (parentMenu != null)
            parentMenu.SetActive(true);

        PlaySound(closeSound);
    }

    private void PlaySound(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound);
    }
}