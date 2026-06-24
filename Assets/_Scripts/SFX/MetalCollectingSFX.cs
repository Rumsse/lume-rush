using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MetalCollectingSFX : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference metalCollectingSound;

    [Header("Detection")]
    [Tooltip("Tag obiektu metalu, np. Metal. Jeœli puste, tag nie bêdzie sprawdzany")]
    [SerializeField] private string metalTag = "Metal";

    [Tooltip("Jeœli resourceName zawiera ten tekst, obiekt zostanie uznany za metal")]
    [SerializeField] private string metalResourceNameContains = "Metal";

    private EventInstance collectingInstance;
    private GameObject currentTarget;

    public bool IsMetalTarget(LongerInteractable target)
    {
        if (target == null)
            return false;

        if (!string.IsNullOrEmpty(metalTag) && target.CompareTag(metalTag))
            return true;

        if (target.resourceData != null)
        {
            string resourceName = target.resourceData.resourceName;

            if (!string.IsNullOrEmpty(resourceName) &&
                resourceName.ToLower().Contains(metalResourceNameContains.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    public void StartMetalCollectingSound(LongerInteractable target)
    {
        if (target == null)
            return;

        if (!IsMetalTarget(target))
            return;

        StopMetalCollectingSound();

        currentTarget = target.gameObject;

        collectingInstance = RuntimeManager.CreateInstance(metalCollectingSound);
        collectingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentTarget.transform));
        collectingInstance.start();
    }

    public void StopMetalCollectingSound()
    {
        if (collectingInstance.isValid())
        {
            collectingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            collectingInstance.release();
        }

        currentTarget = null;
    }

    private void Update()
    {
        if (currentTarget == null)
            return;

        if (!collectingInstance.isValid())
            return;

        collectingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentTarget.transform));
    }

    private void OnDisable()
    {
        StopMetalCollectingSound();
    }

    private void OnDestroy()
    {
        StopMetalCollectingSound();
    }
}