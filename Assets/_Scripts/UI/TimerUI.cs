using System;
using TMPro;
using UnityEngine;

public sealed class TimerUI : MonoBehaviour
{
    #region Serialized

    [SerializeField] TMP_Text _label;
    [SerializeField] bool _countDown = true;
    [SerializeField, Min(0)] int _startSeconds = 60;
    [SerializeField] bool _autoStart = true;
    [SerializeField] bool _useUnscaledTime = false;
    [SerializeField] string _playerTag = "Player";
    [SerializeField] GameObject _gameOverPanel;

    #endregion

    #region State

    IHealth _playerHealth;
    double _time;
    bool _running;
    int _lastShown = int.MinValue;

    #endregion

    #region Lifecycle

    void Awake()
    {
        _label ??= GetComponent<TMP_Text>();

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        CachePlayer();
        TutorialManager.OnTutorialCompleted += HandleTutorialCompleted;

        if (PlayerPrefs.GetInt("TutorialDone", 0) == 0)
        {
            _label.gameObject.SetActive(false);
            return;
        }

        if (_autoStart)
            StartTimer();
        else
            Refresh(true);
    }

    void OnDisable()
    {
        TutorialManager.OnTutorialCompleted -= HandleTutorialCompleted;
    }

    void Update()
    {
        if (!_running)
            return;

        var dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _time += _countDown ? -dt : dt;

        if (_countDown && _time <= 0)
        {
            _time = 0;
            _running = false;

            if (_playerHealth == null)
                CachePlayer();

            if (_playerHealth != null)
                _playerHealth.TakeDamage(_playerHealth.MaxHealth);

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            Time.timeScale = 0f;

            Refresh(true);
            return;
        }

        Refresh();
    }

    #endregion

    #region Control

    public bool IsRunning => _running;

    public void StartTimer()
    {
        _time = _countDown ? _startSeconds : 0;
        _running = true;
        _lastShown = int.MinValue;
        _label.gameObject.SetActive(true);
        Refresh(true);
    }

    public void Pause() => _running = false;

    public void Resume()
    {
        if (_time <= 0 && _countDown)
            return;

        _running = true;
    }

    public void Stop()
    {
        _running = false;
        _time = 0;
        Refresh(true);
    }

    public void SetTimeSeconds(int seconds)
    {
        _time = Math.Max(0, seconds);
        _lastShown = int.MinValue;
        Refresh(true);
    }

    public void AddSeconds(int seconds)
    {
        _time = Math.Max(0, _time + seconds);
        _lastShown = int.MinValue;
        Refresh(true);
    }

    #endregion

    #region Helpers

    void CachePlayer()
    {
        var go = GameObject.FindWithTag(_playerTag);

        if (go != null)
            _playerHealth = go.GetComponent<IHealth>();
    }

    void Refresh(bool force = false)
    {
        if (_label == null)
            return;

        var s = (int)Math.Max(0, Math.Floor(_time));

        if (!force && s == _lastShown)
            return;

        _lastShown = s;

        var m = s / 60;
        var sec = s % 60;
        _label.SetText("{0:00}:{1:00}", m, sec);
    }

    void HandleTutorialCompleted()
    {
        if (_autoStart)
            StartTimer();
        else
        {
            _label.gameObject.SetActive(true);
            Refresh(true);
        }
    }

    #endregion
}