using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioSettingsLoader : MonoBehaviour
{
    public string masterBusPath = "bus:/";
    public string musicBusPath = "bus:/Music";
    public string uiBusPath = "bus:/UI";
    public string sfxBusPath = "bus:/SFX";
    public string ambienceBusPath = "bus:/Ambients";

    private void Awake()
    {
        Bus masterBus = RuntimeManager.GetBus(masterBusPath);
        Bus musicBus = RuntimeManager.GetBus(musicBusPath);
        Bus uiBus = RuntimeManager.GetBus(uiBusPath);
        Bus sfxBus = RuntimeManager.GetBus(sfxBusPath);
        Bus ambienceBus = RuntimeManager.GetBus(ambienceBusPath);

        masterBus.setVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        musicBus.setVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        uiBus.setVolume(PlayerPrefs.GetFloat("UIVolume", 1f));
        sfxBus.setVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
        ambienceBus.setVolume(PlayerPrefs.GetFloat("AmbienceVolume", 1f));
    }
}
