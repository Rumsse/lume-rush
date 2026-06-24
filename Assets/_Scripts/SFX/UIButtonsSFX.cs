using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    [Header("FMOD Event Paths")]
    [SerializeField] private string hoverEventPath = "event:/UI/UI_Button_Hover";
    [SerializeField] private string clickEventPath = "event:/UI/UI_Button_Click";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        PlaySound(hoverEventPath);
    }

    private void PlayClickSound()
    {
        if (button != null && !button.interactable)
            return;

        PlaySound(clickEventPath);
    }

    private void PlaySound(string eventPath)
    {
        if (string.IsNullOrWhiteSpace(eventPath))
            return;

        RuntimeManager.PlayOneShot(eventPath);
    }
}