using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using DG.Tweening;

public class ToolbarManager : MonoBehaviour
{
    #region Properties

    public static ToolbarManager Instance { get; private set; }

    public UIDocument uiDocument;

    [SerializeField] TMP_Text fullToolbarPopup;
    [SerializeField] float popupDuration = 2.5f;
    [SerializeField] float animationDuration = 0.35f;
    [SerializeField] float shakeRotationStrength = 20f;
    [SerializeField] float pulseScale = 1.05f;
    [SerializeField] float pulseSpeed = 0.5f;

    List<ToolbarSlot> slots = new List<ToolbarSlot>();
    RectTransform popupRect;
    Vector2 originalPopupPosition;
    Coroutine popupCoroutine;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        Instance = this;

        if (fullToolbarPopup != null)
        {
            popupRect = fullToolbarPopup.GetComponent<RectTransform>();
            originalPopupPosition = popupRect.anchoredPosition;
            popupRect.localScale = Vector3.zero;
            fullToolbarPopup.gameObject.SetActive(false);
        }

        var root = uiDocument.rootVisualElement;
        var slotElements = root.Query<VisualElement>("Slot").ToList();

        foreach (var elem in slotElements)
            slots.Add(new ToolbarSlot(elem));
    }

    #endregion

    #region Resource Management

    public bool AddResource(ResourceData resourceData)
    {
        if (resourceData == null)
            return false;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(resourceData.resourceName, resourceData.resourceIcon, resourceData);
                Debug.Log($"Resource added {resourceData.resourceName} to the Toolbar.");
                return true;
            }
        }

        ShowFullPopup();
        return false;
    }

    public bool AddResource(string resourceName, Sprite resourceIcon)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(resourceName, resourceIcon, null);
                Debug.Log($"Resource added {resourceName} to the Toolbar.");
                return true;
            }
        }

        ShowFullPopup();
        return false;
    }

    #endregion

    #region Toolbar Logic

    public List<ToolbarSlot> GetSlots() => slots;

    public void ClearToolbar()
    {
        foreach (var slot in slots)
            slot.Clear();
    }

    #endregion

    #region UI Popups

    void ShowFullPopup()
    {
        Debug.LogWarning("No empty slot available in Toolbar!");

        if (fullToolbarPopup == null || popupRect == null)
            return;

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

        fullToolbarPopup.gameObject.SetActive(true);

        popupRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
        popupRect.DOPunchRotation(new Vector3(0f, 0f, shakeRotationStrength), animationDuration * 1.5f, 10, 0.5f);

        yield return new WaitForSeconds(animationDuration * 1.5f);

        var pulse = popupRect.DOScale(new Vector3(pulseScale, pulseScale, 1f), pulseSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        var waitTime = popupDuration - (animationDuration * 1.5f);

        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        pulse.Kill();

        popupRect.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack).OnComplete(() => fullToolbarPopup.gameObject.SetActive(false));

        popupCoroutine = null;
    }

    #endregion
}