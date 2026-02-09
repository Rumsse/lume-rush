using System.IO;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class SFX_UIButtonSoundManager : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private EventReference uiHoverSound = EventReference.Find("event:/SFX/UI/UI_Button_Hover");
    [SerializeField] private EventReference uiClickSound = EventReference.Find("event:/SFX/UI/UI_Button_Click");

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
