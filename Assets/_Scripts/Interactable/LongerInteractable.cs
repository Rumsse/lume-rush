using UnityEngine;

public class LongerInteractable : Interactable
{
    [SerializeField] float collectTime = 1.5f;
    [SerializeField] GameObject particleEffect;

    public float CollectTime => collectTime;

    void Start()
    {
        if (particleEffect == null)
            particleEffect = GetComponentInChildren<ParticleSystem>()?.gameObject;

        if (particleEffect != null)
            particleEffect.SetActive(false);
    }

    public void StartCollecting()
    {
        if (particleEffect != null)
            particleEffect.SetActive(true);
    }

    public void StopCollecting()
    {
        if (particleEffect != null)
            particleEffect.SetActive(false);
    }
}