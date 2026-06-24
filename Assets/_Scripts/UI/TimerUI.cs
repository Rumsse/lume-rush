using TMPro;
using UnityEngine;

public sealed class TimerUI : MonoBehaviour
{
    #region Settings

    [SerializeField] TMP_Text label;

    #endregion

    #region State

    int lastShown = int.MinValue;
    bool isActive;

    #endregion

    #region Unity Callbacks

    void Awake() => label ??= GetComponent<TMP_Text>();

    void OnEnable()
    {
        TutorialManager.OnTutorialCompleted += HandleTutorialCompleted;
        UpdateVisibility();
    }

    void OnDisable() => TutorialManager.OnTutorialCompleted -= HandleTutorialCompleted;

    void Update()
    {
        if (!isActive)
            return;

        Refresh();
    }

    #endregion

    #region Logic

    void UpdateVisibility()
    {
        if (PlayerPrefs.GetInt("TutorialDone", 0) == 0 || SupernovaManager.Instance == null)
        {
            isActive = false;
            label.gameObject.SetActive(false);
            return;
        }

        isActive = true;
        label.gameObject.SetActive(true);
    }

    void Refresh()
    {
        if (label == null || SupernovaManager.Instance == null)
            return;

        var playerPos = PlanetSelectionManager.CurrentPlayerPosition;
        var currentRadius = SupernovaManager.Instance.CurrentRadius;
        var speed = SupernovaManager.Instance.ExpansionSpeed;

        var distanceToPlayer = Vector3.Distance(Vector3.zero, playerPos);
        var distanceRemaining = distanceToPlayer - currentRadius;

        var timeRemaining = speed > 0f ? distanceRemaining / speed : 0f;
        timeRemaining = Mathf.Max(0f, timeRemaining);

        var s = Mathf.FloorToInt(timeRemaining);

        if (s == lastShown)
            return;

        lastShown = s;

        var m = s / 60;
        var sec = s % 60;
        label.SetText("{0:00}:{1:00}", m, sec);
    }

    void HandleTutorialCompleted() => UpdateVisibility();

    #endregion
}