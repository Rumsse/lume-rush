using UnityEngine;
using FMODUnity;

public class AudioMenuToggle : MonoBehaviour
{
    public GameObject audioSettingsMenu;

    public EventReference openSound;
    public EventReference closeSound;

    private void Start()
    {
        if (audioSettingsMenu != null)
            audioSettingsMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            ToggleAudioSettings();
    }

    public void ToggleAudioSettings()
    {
        if (audioSettingsMenu == null)
            return;

        bool open = audioSettingsMenu.activeSelf;

        audioSettingsMenu.SetActive(!open);

        if (!open)
        {
            PlaySound(openSound);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Time.timeScale = 0f;
        }
        else
        {
            PlaySound(closeSound);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Time.timeScale = 1f;
        }
    }

    private void PlaySound(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound);
    }
}