using UnityEngine;
using UnityEngine.InputSystem;

public class SlotController : MonoBehaviour
{
    public bool _selected;
    public ScriptableObject stationData;
    public SlotInteraction SI;
    public GameObject panelUI;
    public Vector2 mousePos;
    public Camera mainCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OpenBuildUI()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (_selected == true)
            {
                panelUI.transform.position = new Vector2(Mouse.current.position.x.ReadValue() + (panelUI.GetComponent<RectTransform>().rect.size.x / 2), Mouse.current.position.y.ReadValue() - (panelUI.GetComponent<RectTransform>().rect.size.y / 2));
                panelUI.SetActive(true);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        OpenBuildUI();
    }
}
