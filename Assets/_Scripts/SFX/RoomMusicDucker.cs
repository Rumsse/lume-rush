using System;
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RoomMusicDucker : MonoBehaviour
{
    [Serializable]
    public class RoomAmbientEntry
    {
        public RoomType roomType;
        public EventReference ambientEvent;
    }

    [Header("Room")]
    [SerializeField] private RoomSlot roomSlot;

    [Header("FMOD Emitter")]
    [SerializeField] private StudioEventEmitter roomEmitter;

    [Header("Room Type Ambients")]
    [SerializeField] private RoomAmbientEntry[] roomAmbients;

    [Header("FMOD Music Bus")]
    [SerializeField] private string musicBusPath = "bus:/Music";

    [Header("Music Ducking")]
    [Range(0f, 1f)]
    [SerializeField] private float duckedMusicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float normalMusicVolume = 1f;

    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private Bus musicBus;
    private Coroutine volumeFadeCoroutine;

    private bool playerInside;
    private bool isDucking;
    private RoomType lastRoomType = RoomType.None;

    private void Awake()
    {
        musicBus = RuntimeManager.GetBus(musicBusPath);

        if (roomSlot == null)
            roomSlot = GetComponentInParent<RoomSlot>();

        if (roomEmitter == null)
            roomEmitter = GetComponentInChildren<StudioEventEmitter>(true);
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (roomSlot == null)
            return;

        if (roomSlot.CurrentRoomType != lastRoomType)
            RefreshRoomAmbient();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;
        RefreshRoomAmbient();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;
        StopRoomAmbient();
        RestoreMusicVolume();
    }

    private void RefreshRoomAmbient()
    {
        if (roomSlot == null)
            return;

        lastRoomType = roomSlot.CurrentRoomType;

        if (roomSlot.CurrentRoomType == RoomType.None)
        {
            StopRoomAmbient();
            RestoreMusicVolume();
            return;
        }

        EventReference ambientEvent = GetAmbientForRoomType(roomSlot.CurrentRoomType);

        if (ambientEvent.IsNull)
        {
            StopRoomAmbient();
            RestoreMusicVolume();
            return;
        }

        PlayRoomAmbient(ambientEvent);
        DuckMusicVolume();
    }

    private EventReference GetAmbientForRoomType(RoomType roomType)
    {
        if (roomAmbients == null)
            return default;

        foreach (RoomAmbientEntry entry in roomAmbients)
        {
            if (entry == null)
                continue;

            if (entry.roomType == roomType)
                return entry.ambientEvent;
        }

        return default;
    }

    private void PlayRoomAmbient(EventReference ambientEvent)
    {
        if (roomEmitter == null)
            return;

        roomEmitter.Stop();
        roomEmitter.EventReference = ambientEvent;
        roomEmitter.Play();
    }

    private void StopRoomAmbient()
    {
        if (roomEmitter != null)
            roomEmitter.Stop();
    }

    private void DuckMusicVolume()
    {
        FadeMusicVolumeTo(duckedMusicVolume, fadeOutDuration);
        isDucking = true;
    }

    private void RestoreMusicVolume()
    {
        if (!isDucking)
            return;

        FadeMusicVolumeTo(normalMusicVolume, fadeInDuration);
        isDucking = false;
    }

    private void FadeMusicVolumeTo(float targetVolume, float duration)
    {
        if (!musicBus.isValid())
            return;

        if (volumeFadeCoroutine != null)
            StopCoroutine(volumeFadeCoroutine);

        volumeFadeCoroutine = StartCoroutine(FadeMusicVolumeRoutine(targetVolume, duration));
    }

    private IEnumerator FadeMusicVolumeRoutine(float targetVolume, float duration)
    {
        musicBus.getVolume(out float startVolume);

        if (duration <= 0f)
        {
            musicBus.setVolume(targetVolume);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float t = elapsedTime / duration;
            float volume = Mathf.Lerp(startVolume, targetVolume, t);

            musicBus.setVolume(volume);

            yield return null;
        }

        musicBus.setVolume(targetVolume);
        volumeFadeCoroutine = null;
    }

    private void OnDisable()
    {
        StopRoomAmbient();

        if (volumeFadeCoroutine != null)
            StopCoroutine(volumeFadeCoroutine);

        if (musicBus.isValid())
            musicBus.setVolume(normalMusicVolume);

        isDucking = false;
    }

    private void OnDestroy()
    {
        StopRoomAmbient();

        if (volumeFadeCoroutine != null)
            StopCoroutine(volumeFadeCoroutine);

        if (musicBus.isValid())
            musicBus.setVolume(normalMusicVolume);

        isDucking = false;
    }
}