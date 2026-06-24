using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    [Header("SFX")]
    [SerializeField]
    private EventReference hoverSound =
    EventReference.Find("event:/UI/UI_Button_Hover");

    [SerializeField]
    private EventReference clickSound =
        EventReference.Find("event:/UI/UI_Button_Click");

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

        PlaySound(hoverSound);
    }

    private void PlayClickSound()
    {
        if (button != null && !button.interactable)
            return;

        PlaySound(clickSound);
    }

    private void PlaySound(EventReference sound)
    {
        if (!sound.IsNull)
            RuntimeManager.PlayOneShot(sound);
    }
}