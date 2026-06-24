using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PumpSFXController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference pumpPlaceSound;
    [SerializeField] private EventReference pumpWorkingSound;
    [SerializeField] private EventReference pumpPickUpSound;

    private EventInstance workingInstance;
    private GameObject currentPump;

    public void PlayPlaceSound(Vector3 position)
    {
        RuntimeManager.PlayOneShot(pumpPlaceSound, position);
    }

    public void StartWorkingSound(GameObject pump)
    {
        if (pump == null)
            return;

        StopWorkingSound();

        currentPump = pump;

        workingInstance = RuntimeManager.CreateInstance(pumpWorkingSound);
        workingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentPump.transform));
        workingInstance.start();
    }

    public void StopWorkingSound()
    {
        if (workingInstance.isValid())
        {
            workingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            workingInstance.release();
        }

        currentPump = null;
    }

    public void PlayPickUpSound(Vector3 position)
    {
        RuntimeManager.PlayOneShot(pumpPickUpSound, position);
    }

    private void Update()
    {
        if (currentPump == null)
            return;

        if (!workingInstance.isValid())
            return;

        workingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentPump.transform));
    }

    private void OnDisable()
    {
        StopWorkingSound();
    }

    private void OnDestroy()
    {
        StopWorkingSound();
    }
}