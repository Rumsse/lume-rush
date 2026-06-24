using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MiningSFXController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference miningLoopSound;

    private EventInstance miningInstance;
    private GameObject currentTarget;

    public void StartMiningSound(GameObject target)
    {
        if (target == null)
            return;

        StopMiningSound();

        currentTarget = target;

        miningInstance = RuntimeManager.CreateInstance(miningLoopSound);
        miningInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentTarget.transform));
        miningInstance.start();
    }

    public void StopMiningSound()
    {
        if (miningInstance.isValid())
        {
            miningInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            miningInstance.release();
        }

        currentTarget = null;
    }

    private void Update()
    {
        if (currentTarget == null)
            return;

        if (!miningInstance.isValid())
            return;

        miningInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentTarget.transform));
    }

    private void OnDisable()
    {
        StopMiningSound();
    }

    private void OnDestroy()
    {
        StopMiningSound();
    }
}