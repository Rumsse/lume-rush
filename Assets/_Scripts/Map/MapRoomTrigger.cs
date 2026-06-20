using UnityEngine;

public class MapRoomTrigger : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] GameObject confirmationPanel;
    [SerializeField] GameObject interactionPrompt;
    [SerializeField] string playerTag = "Player";
    [SerializeField] KeyCode interactKey = KeyCode.E;

    #endregion

    #region State

    bool playerInRange;

    #endregion

    #region Unity Callbacks

    void Start()
    {
        if (confirmationPanel)
            confirmationPanel.SetActive(false);

        if (interactionPrompt)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
            ShowConfirmationPanel();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (interactionPrompt)
            interactionPrompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
        HideConfirmationPanel();

        if (interactionPrompt)
            interactionPrompt.SetActive(false);
    }

    #endregion

    #region Panel Methods

    void ShowConfirmationPanel()
    {
        if (!confirmationPanel)
            return;

        confirmationPanel.SetActive(true);
        Time.timeScale = 0f;
/*        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;*/

        if (interactionPrompt)
            interactionPrompt.SetActive(false);
    }

    void HideConfirmationPanel()
    {
        if (!confirmationPanel)
            return;

        confirmationPanel.SetActive(false);
        Time.timeScale = 1f;
/*        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
    }

    public void OnYesButton()
    {
        Time.timeScale = 1f;

        if (SceneLoader.Instance)
            SceneLoader.Instance.LoadMap();
    }

    public void OnNoButton() => HideConfirmationPanel();

    #endregion
}