using UnityEngine;

public class PlanetInteractable : Interactable
{
    public Transform spawnPanelPosition;
    private GameObject spawnedPanel;

    void OnMouseEnter()
    {
        transform.localScale *= 1.2f;
        SetHighlighted(true);
    }

    void OnMouseExit()
    {
        transform.localScale /= 1.2f;
        SetHighlighted(false);
    }

    void OnMouseDown()
    {
        Debug.Log("Planet clicked: " + name);

        if (spawnedPanel != null) return;

        var globalPanel = GameObject.FindObjectOfType<PlanetConfirmationUI>();
        if (globalPanel == null)
        {
            Debug.LogError("Global Confirmation Panel not found in the scene!");
            return;
        }

        spawnedPanel = globalPanel.gameObject;
        spawnedPanel.transform.position = spawnPanelPosition.position;
        spawnedPanel.SetActive(true);

        var confirmationUI = spawnedPanel.GetComponent<PlanetConfirmationUI>();
        Debug.Log("Setting up the Confirmation Panel for: " + ResourceName);
        confirmationUI.Setup(
            ResourceName,
            OnConfirmAction,
            OnCancelAction
        );
    }

    void OnConfirmAction()
    {
        PlayerPrefs.SetString("SelectedPlanet", ResourceName);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Test Planet Exploration");
    }

    void OnCancelAction()
    {
        if (spawnedPanel != null) spawnedPanel.SetActive(false);
    }
}