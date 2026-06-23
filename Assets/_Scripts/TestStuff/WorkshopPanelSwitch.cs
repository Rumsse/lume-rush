using UnityEngine;

public class WorkshopPanelSwitch : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] GameObject workshopPanel;
    [SerializeField] GameObject interactionPrompt;
    [SerializeField] KeyCode interactKey = KeyCode.E;

    #endregion

    #region State

    bool playerInRange;
    bool isPanelOpen;

    #endregion

    #region Unity Callbacks

    void OnEnable() => WorkshopPlayerDetection.OnWorkshopRangeChanged += HandlePlayerRangeChanged;

    void OnDisable() => WorkshopPlayerDetection.OnWorkshopRangeChanged -= HandlePlayerRangeChanged;

    void Start()
    {
        if (workshopPanel)
            workshopPanel.SetActive(false);

        if (interactionPrompt)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        if (isPanelOpen)
        {
            CloseWorkshop();
            return;
        }

        if (playerInRange)
            OpenWorkshop();
    }

    #endregion

    #region Logic

    void HandlePlayerRangeChanged(bool inRange)
    {
        playerInRange = inRange;

        if (!inRange)
        {
            CloseWorkshop();

            if (interactionPrompt)
                interactionPrompt.SetActive(false);

            return;
        }

        if (interactionPrompt && !isPanelOpen)
            interactionPrompt.SetActive(true);
    }

    void OpenWorkshop()
    {
        if (!workshopPanel)
            return;

        isPanelOpen = true;
        workshopPanel.SetActive(true);
        Time.timeScale = 0f;

        if (interactionPrompt)
            interactionPrompt.SetActive(false);
    }

    void CloseWorkshop()
    {
        if (!workshopPanel)
            return;

        isPanelOpen = false;
        workshopPanel.SetActive(false);
        Time.timeScale = 1f;

        if (interactionPrompt && playerInRange)
            interactionPrompt.SetActive(true);
    }

    #endregion
}