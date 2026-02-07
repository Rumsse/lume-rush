using UnityEngine;

public class MapRoomTrigger : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    #endregion

    #region Private Fields

    private bool playerInRange;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
            ShowConfirmationPanel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            HideConfirmationPanel();
        }
    }

    #endregion

    #region Panel Methods

    private void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HideConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnYesButton()
    {
        Time.timeScale = 1f;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadMap();
    }

    public void OnNoButton() => HideConfirmationPanel();

    #endregion
}