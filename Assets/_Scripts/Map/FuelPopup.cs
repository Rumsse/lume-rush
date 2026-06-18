using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FuelPopup : MonoBehaviour
{
    #region Settings

    public static FuelPopup Instance { get; private set; }

    [SerializeField] TMP_Text popupText;
    [SerializeField] float popupDuration = 2.5f;
    [SerializeField] float animationDuration = 0.35f;
    [SerializeField] float shakeRotationStrength = 20f;
    [SerializeField] float pulseScale = 1.05f;
    [SerializeField] float pulseSpeed = 0.5f;

    #endregion

    #region State

    RectTransform popupRect;
    Vector2 originalPopupPosition;
    Coroutine popupCoroutine;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (popupText != null)
        {
            popupRect = popupText.GetComponent<RectTransform>();
            originalPopupPosition = popupRect.anchoredPosition;
            popupRect.localScale = Vector3.zero;
            popupText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Logic

    public void ShowMessage(string message)
    {
        if (popupText == null || popupRect == null)
            return;

        popupText.text = message;

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(PopupRoutine());
    }

    IEnumerator PopupRoutine()
    {
        popupRect.DOKill();

        popupRect.localScale = Vector3.zero;
        popupRect.localEulerAngles = Vector3.zero;
        popupRect.anchoredPosition = originalPopupPosition;

        popupText.gameObject.SetActive(true);

        popupRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
        popupRect.DOPunchRotation(new Vector3(0f, 0f, shakeRotationStrength), animationDuration * 1.5f, 10, 0.5f);

        yield return new WaitForSeconds(animationDuration * 1.5f);

        var pulse = popupRect.DOScale(new Vector3(pulseScale, pulseScale, 1f), pulseSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        var waitTime = popupDuration - (animationDuration * 1.5f);

        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        pulse.Kill();

        popupRect.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack).OnComplete(() => popupText.gameObject.SetActive(false));

        popupCoroutine = null;
    }

    #endregion
}