using UnityEngine;

public class WorkshopPanelSwitch : MonoBehaviour
{
    public GameObject workshopPanel;
    void Update()
    {
        if (workshopPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E)) workshopPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
