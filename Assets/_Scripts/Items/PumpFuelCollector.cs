using UnityEngine;
using UnityEngine.UI;

public class PumpFuelCollector : MonoBehaviour
{
    [SerializeField] float collectDuration = 30f;
    [SerializeField] int fuelAmount = 1;
    [SerializeField] GameObject sliderPrefab;
    [SerializeField] Vector3 sliderOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] bool faceCamera = true;

    GameObject uiInstance;
    Transform uiTransform;
    Slider slider;
    Image fillImage;
    Camera cam;

    float timer;
    bool collecting;
    bool charged;
    bool producedOnce;

    public bool IsCharged => charged;
    public bool ProducedOnce => producedOnce;
    public int FuelAmount => charged ? fuelAmount : 0;

    void Awake() => cam = Camera.main;

    void Update()
    {
        if (collecting)
        {
            timer += Time.deltaTime;
            var f = Mathf.Clamp01(timer / collectDuration);
            UpdateUI(f);
            if (timer >= collectDuration) { collecting = false; charged = true; }
        }

        if (faceCamera) Billboard();
    }

    public void BeginCollect()
    {
        if (producedOnce) return;
        charged = false;
        timer = 0f;
        collecting = true;
        EnsureUI();
        SetUIActive(true);
        UpdateUI(0f);
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
}