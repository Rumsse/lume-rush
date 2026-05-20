using UnityEngine;

public class WorkshopPanelSwitch : MonoBehaviour
{
    public GameObject workshopPanel;
    private void Start()
    {
        workshopPanel.SetActive(false);
    }
    void Update()
    {
        if (!workshopPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E)) workshopPanel.SetActive(true);
        }
        else if (workshopPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E)) workshopPanel.SetActive(false);
        }
    }
}
