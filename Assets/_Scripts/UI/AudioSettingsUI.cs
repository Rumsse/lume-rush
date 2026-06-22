using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider uiSlider;
    public Slider sfxSlider;
    public Slider ambienceSlider;

    [Header("Menus")]
    public GameObject audioSettingsMenu;
    public GameObject pauseMenu;

    [Header("FMOD Bus Paths")]
    public string masterBusPath = "bus:/";
    public string musicBusPath = "bus:/Music";
    public string uiBusPath = "bus:/UI";
    public string sfxBusPath = "bus:/SFX";
    public string ambienceBusPath = "bus:/Ambients";

    private Bus masterBus;
    private Bus musicBus;
    private Bus uiBus;
    private Bus sfxBus;
    private Bus ambienceBus;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string UIVolumeKey = "UIVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string AmbienceVolumeKey = "AmbienceVolume";

    private void Awake()
    {
        masterBus = RuntimeManager.GetBus(masterBusPath);
        musicBus = RuntimeManager.GetBus(musicBusPath);
        uiBus = RuntimeManager.GetBus(uiBusPath);
        sfxBus = RuntimeManager.GetBus(sfxBusPath);
        ambienceBus = RuntimeManager.GetBus(ambienceBusPath);

        LoadVolumes();
        AddSliderListeners();
    }

    private void OnEnable()
    {
        LoadVolumes();
    }

    private void AddSliderListeners()
    {
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        ambienceSlider.onValueChanged.AddListener(SetAmbienceVolume);
    }

    private void LoadVolumes()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float uiVolume = PlayerPrefs.GetFloat(UIVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        float ambienceVolume = PlayerPrefs.GetFloat(AmbienceVolumeKey, 1f);

        masterSlider.SetValueWithoutNotify(masterVolume);
        musicSlider.SetValueWithoutNotify(musicVolume);
        uiSlider.SetValueWithoutNotify(uiVolume);
        sfxSlider.SetValueWithoutNotify(sfxVolume);
        ambienceSlider.SetValueWithoutNotify(ambienceVolume);

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        masterBus.setVolume(masterSlider.value);
        musicBus.setVolume(musicSlider.value);
        uiBus.setVolume(uiSlider.value);
        sfxBus.setVolume(sfxSlider.value);
        ambienceBus.setVolume(ambienceSlider.value);
    }

    public void SetMasterVolume(float value)
    {
        masterBus.setVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        musicBus.setVolume(value);
    }

    public void SetUIVolume(float value)
    {
        uiBus.setVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        sfxBus.setVolume(value);
    }

    public void SetAmbienceVolume(float value)
    {
        ambienceBus.setVolume(value);
    }

    public void BackToPauseMenu()
    {
        SaveVolumes();

        if (audioSettingsMenu != null)
            audioSettingsMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterSlider.value);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicSlider.value);
        PlayerPrefs.SetFloat(UIVolumeKey, uiSlider.value);
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxSlider.value);
        PlayerPrefs.SetFloat(AmbienceVolumeKey, ambienceSlider.value);

        PlayerPrefs.Save();
    }

    private void OnDisable()
    {
        SaveVolumes();
    }

}