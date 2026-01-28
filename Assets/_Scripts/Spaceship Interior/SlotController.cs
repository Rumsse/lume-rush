using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotController : MonoBehaviour
{
    public bool _selected;
    public SlotDataSO stationData;
    public SlotInteraction SI;
    public GameObject panelUI;
    public Vector2 mousePos;
    public Camera mainCam;
    public StatsDisplay SD;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OpenBuildUI()
    {
        if (_selected == true)
        {
            panelUI.transform.position = new Vector2(Mouse.current.position.x.ReadValue() + (panelUI.GetComponent<RectTransform>().rect.size.x / 2) - 5, Mouse.current.position.y.ReadValue() - (panelUI.GetComponent<RectTransform>().rect.size.y / 2) + 5 );

            if(panelUI.activeInHierarchy == false)
            {
                SD.UpdateStats();
                SD.DisplayStats();
                panelUI.SetActive(true);
            }
        }
        else if (_selected == false && panelUI.activeInHierarchy == true)
        {
            foreach (TextMeshProUGUI stat in SD.statList)
            {
                Destroy(stat.gameObject);
            }
            SD.statList.Clear();
            panelUI.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        OpenBuildUI();
    }
}
