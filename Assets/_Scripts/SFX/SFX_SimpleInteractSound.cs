using System.Runtime.CompilerServices;
using FMODUnity;
using UnityEngine;

public class SFX_SimpleSoundInteract : MonoBehaviour
{
    [Header("FMOD Settings")]
    [SerializeField] private EventReference itemPickUpSound;
    [SerializeField] private EventReference pumpPlaceSound;
    [SerializeField] private EventReference pumpWorkingSound;
    [SerializeField] private EventReference uiHoverSound;
    [SerializeField] private EventReference uiClickSound;
    



    public void PlaySound_ItemPickUp()
    {
        RuntimeManager.PlayOneShot(itemPickUpSound, transform.position);
    }

    public void PlaySound_PumpPlace()
    {
        RuntimeManager.PlayOneShot(pumpPlaceSound, transform.position);
    }

    public void PlaySound_PumpWorkng(bool turnOn, GameObject pump)
    {
        FMOD.Studio.EventInstance soundInstance = RuntimeManager.CreateInstance(pumpWorkingSound);
        if (turnOn)
        {
            soundInstance.start();
            RuntimeManager.AttachInstanceToGameObject(soundInstance, pump);
            soundInstance.set3DAttributes(pump.transform.position.To3DAttributes());
        }
        else if (!turnOn)
        {
            soundInstance.release();
        }
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