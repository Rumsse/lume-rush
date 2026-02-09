using System.IO;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class SFX_UIButtonSoundManager : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private EventReference uiHoverSound;
    [SerializeField] private EventReference uiClickSound;

    void Awake()
    {
        uiHoverSound = EventReference.Find("event:/SFX/UI/UI_Button_Hover");
        uiClickSound = EventReference.Find("event:/SFX/UI/UI_Button_Click");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(uiHoverSound);
    }

    public void OnClickSFX()
    {
        RuntimeManager.PlayOneShot(uiClickSound);
    }
    public void PlaySound_UIHover()
    {
        RuntimeManager.PlayOneShot(uiHoverSound, transform.position);
    }

    public void PlaySound_UIClick()
    {
        RuntimeManager.PlayOneShot(uiClickSound, transform.position);
    }
}
