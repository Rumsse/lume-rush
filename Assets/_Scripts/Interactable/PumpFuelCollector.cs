using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PumpFuelCollector : MonoBehaviour
{
    #region Serialized

    [SerializeField] float collectDuration = 30f;
    [SerializeField] int fuelAmount = 1;
    [SerializeField] GameObject sliderPrefab;
    [SerializeField] Vector3 sliderOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] bool faceCamera = true;
    [SerializeField] Transform pumpModel;
    [SerializeField] float pulseDuration = 0.6f;
    [SerializeField] Vector3 pulseScale = new Vector3(1.15f, 0.95f, 1.15f);
    [SerializeField] Ease pulseEaseOut = Ease.OutQuad;
    [SerializeField] Ease pulseEaseIn = Ease.InQuad;

    #endregion

    #region UI & References

    GameObject uiInstance;
    Transform uiTransform;
    Slider slider;
    Image fillImage;
    Camera cam;

    #endregion

    #region State

    float timer;
    bool collecting;
    bool charged;
    bool producedOnce;
    Sequence pulseSequence;
    Vector3 originalScale;

    #endregion

    #region Properties

    public bool IsCharged => charged;
    public bool ProducedOnce => producedOnce;
    public int FuelAmount => charged ? fuelAmount : 0;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        cam = Camera.main;
        if (pumpModel) originalScale = pumpModel.localScale;
    }

    void Update()
    {
        if (collecting)
        {
            timer += Time.deltaTime;
            var f = Mathf.Clamp01(timer / collectDuration);
            UpdateUI(f);

            if (timer >= collectDuration)
            {
                collecting = false;
                charged = true;
                StopPulse();
            }
        }

        if (faceCamera) Billboard();
    }

    void OnDestroy() => pulseSequence?.Kill();

    #endregion

    #region API

    public void BeginCollect()
    {
        if (producedOnce) return;

        charged = false;
        timer = 0f;
        collecting = true;
        EnsureUI();
        SetUIActive(true);
        UpdateUI(0f);
        StartPulse();
    }

    public int ConsumeFuel()
    {
        if (!charged) return 0;

        producedOnce = true;
        charged = false;
        collecting = false;
        SetUIActive(false);
        return fuelAmount;
    }

    #endregion

    #region Pulse Animation

    void StartPulse()
    {
        if (!pumpModel) return;

        StopPulse();

        var targetScale = new Vector3(
            originalScale.x * pulseScale.x,
            originalScale.y * pulseScale.y,
            originalScale.z * pulseScale.z
        );

        pulseSequence = DOTween.Sequence()
            .Append(pumpModel.DOScale(targetScale, pulseDuration * 0.4f).SetEase(pulseEaseOut))
            .Append(pumpModel.DOScale(originalScale, pulseDuration * 0.6f).SetEase(pulseEaseIn))
            .SetLoops(-1);
    }

    void StopPulse()
    {
        if (!pumpModel) return;

        pulseSequence?.Kill();
        pumpModel.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
    }

    #endregion

    #region UI

    void EnsureUI()
    {
        if (uiInstance) return;
        if (!sliderPrefab) return;

        uiInstance = Instantiate(sliderPrefab, transform);
        uiTransform = uiInstance.transform;
        uiTransform.localPosition = sliderOffset;

        slider = uiInstance.GetComponentInChildren<Slider>(true);
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 0f;
        }
        else
        {
            fillImage = uiInstance.GetComponentInChildren<Image>(true);
            if (fillImage) fillImage.fillAmount = 0f;
        }
    }

    void UpdateUI(float f)
    {
        if (!uiInstance) EnsureUI();
        if (slider) slider.value = f;
        else if (fillImage) fillImage.fillAmount = f;
    }

    void SetUIActive(bool v)
    {
        if (!uiInstance) return;
        uiInstance.SetActive(v);
    }

    void Billboard()
    {
        if (!uiTransform) return;
        if (!cam || !cam.isActiveAndEnabled) cam = Camera.main;
        if (!cam) return;

        var toCam = cam.transform.position - uiTransform.position;
        if (toCam.sqrMagnitude > 1e-6f) uiTransform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
    }

    #endregion
}