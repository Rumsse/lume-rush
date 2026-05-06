using UnityEngine;
using System;
using TMPro;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager Instance { get; private set; }

    [SerializeField] GameObject tutorialPanel;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] bool enableTutorial = true;

    TutorialStep currentStep;
    bool isCompleted;
    float wsadMoveTimer;

    public static Action OnTutorialCompleted;

    #region Unity Callbacks

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadState();
    }

    void Update()
    {
        if (isCompleted)
            return;

        CheckInputSteps();
    }

    #endregion

    #region Logic

    void LoadState()
    {
        if (PlayerPrefs.GetInt("TutorialDone", 0) == 1 || !enableTutorial)
        {
            isCompleted = true;
            UpdateUI();
            return;
        }

        currentStep = TutorialStep.MoveWASD;
        UpdateUI();
    }

    void CheckInputSteps()
    {
        if (currentStep == TutorialStep.MoveWASD)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                wsadMoveTimer += Time.deltaTime;
                if (wsadMoveTimer >= 3f)
                    AdvanceStep(TutorialStep.MoveWASD);
            }
        }

        if (currentStep == TutorialStep.Sprint)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                AdvanceStep(TutorialStep.Sprint);
        }
    }

    public void ReportAction(TutorialStep action)
    {
        if (isCompleted)
            return;

        if (currentStep == action)
            AdvanceStep(action);
    }

    void AdvanceStep(TutorialStep step)
    {
        currentStep = (TutorialStep)((int)currentStep + 1);

        if (currentStep == TutorialStep.Completed)
        {
            isCompleted = true;
            PlayerPrefs.SetInt("TutorialDone", 1);
            PlayerPrefs.Save();
            OnTutorialCompleted?.Invoke();
        }

        UpdateUI();
    }

    #endregion

    #region UI

    void UpdateUI()
    {
        if (isCompleted)
        {
            if (tutorialPanel)
                tutorialPanel.SetActive(false);

            return;
        }

        if (tutorialPanel && !tutorialPanel.activeSelf)
            tutorialPanel.SetActive(true);

        if (!tutorialText)
            return;

        tutorialText.text = currentStep switch
        {
            TutorialStep.MoveWASD => "Move around (W, A, S, D)",
            TutorialStep.PickUpFood => "Pick up any food [E]",
            TutorialStep.MineStone => "Mine any stone (Hold [E])",
            TutorialStep.PlacePump => "Click [F] then when the green pump outline is available click [LMB] to place it. You can't place it when you're too close to the ship - the outline would appear red then.",
            TutorialStep.PickUpPump => "After the bar will be full, and pump will stop working, pick up pump [E]",
            TutorialStep.Sprint => "Use sprint [SHIFT] to move faster. Remember that it has a few seconds cooldown!",
            TutorialStep.DepositShip => "Go back to your ship and deposit items into the ship [E]",
            TutorialStep.LeavePlanet => "Escape from planet [Q]. You need to stand near your ship.",
            _ => ""
        };
    }

    #endregion
}

public enum TutorialStep
{
    MoveWASD,
    PickUpFood,
    MineStone,
    PlacePump,
    PickUpPump,
    Sprint,
    DepositShip,
    LeavePlanet,
    Completed
}