using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FMODUnity;
using UnityEngine.Events;

public class WorkshopButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public string WorkshopItemID { get; private set; }

    [Header("State Visuals")]
    [SerializeField] private GameObject canBuyGlowObject;
    [SerializeField] private GameObject cantBuyObject;
    [SerializeField] private GameObject doneObject;

    [Header("Completion Panel")]
    [SerializeField] private GameObject allItemsBoughtPanel;

    [Tooltip("Jeœli true, panel pojawi siê ponownie po wejœciu do warsztatu, jeœli wszystko zosta³o ju¿ kupione.")]
    [SerializeField] private bool showCompletionPanelOnEnable = true;

    [Header("Cost")]
    public BuyCostSO materialsNeeded;
    public TextMeshProUGUI[] costTexts;

    [Header("SFX")]
    public EventReference hoverSound;
    public EventReference buySound;
    public EventReference notEnoughMaterialsSound;

    [Header("Events")]
    public UnityEvent onBought;
    public UnityEvent onAllWorkshopItemsBought;

    private static bool allWorkshopItemsBought;

    private bool canBuy;
    private bool isBought;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        ApplyVisualState();

        if (allItemsBoughtPanel != null)
            allItemsBoughtPanel.SetActive(false);
    }

    private void OnEnable()
    {
        LoadSavedState();
        StartCoroutine(WaitForResourceManagerAndRefresh());

        if (showCompletionPanelOnEnable && allWorkshopItemsBought)
            ShowAllItemsBoughtPanel();
    }

    private IEnumerator WaitForResourceManagerAndRefresh()
    {
        while (ResourceManager.Instance == null)
            yield return null;

        RefreshData();

        if (showCompletionPanelOnEnable && allWorkshopItemsBought)
            ShowAllItemsBoughtPanel();
    }

    private void LoadSavedState()
    {
        if (string.IsNullOrEmpty(WorkshopItemID))
        {
            Debug.LogWarning($"{name}: WorkshopItemID is empty!");
            return;
        }

        if (ShipSaveManager.IsWorkshopItemBought(WorkshopItemID))
        {
            isBought = true;
            Button button = GetComponent<Button>();

            if (button != null)
                button.interactable = false;

            ApplyVisualState();
        }
    }

    public void BuyItem()
    {
        RefreshData();

        if (isBought || ResourceManager.Instance == null)
            return;

        if (!canBuy)
        {
            RuntimeManager.PlayOneShot(notEnoughMaterialsSound);
            return;
        }

        if (materialsNeeded == null || materialsNeeded.items == null)
        {
            Debug.LogWarning($"{name}: materialsNeeded albo items nie s¹ przypiête!");
            return;
        }

        foreach (var item in materialsNeeded.items)
        {
            if (item == null || item.resource == null)
                continue;

            ResourceManager.Instance.ConsumeResource(
                item.resource.GetStorageName(),
                item.resourceAmount
            );
        }

        RuntimeManager.PlayOneShot(buySound);

        isBought = true;

        Button button = GetComponent<Button>();
        if (button != null)
            button.interactable = false;

        ApplyVisualState();
        ShipSaveManager.SaveWorkshopItem(WorkshopItemID);

        onBought?.Invoke();

        RefreshAllWorkshopButtons();
        CheckAllWorkshopItemsBought();
    }

    public void RefreshData()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning($"{name}: ResourceManager.Instance == null");
            return;
        }

        if (materialsNeeded == null)
        {
            Debug.LogWarning($"{name}: materialsNeeded nie jest przypiête w Inspectorze");
            return;
        }

        if (materialsNeeded.items == null || materialsNeeded.items.Length == 0)
        {
            Debug.LogWarning($"{name}: materialsNeeded.items jest puste");
            return;
        }

        if (costTexts == null || costTexts.Length == 0)
        {
            Debug.LogWarning($"{name}: costTexts nie s¹ przypiête w Inspectorze");
            return;
        }

        CostTextFill();
        CheckIfCanBuy();
        ApplyVisualState();
    }

    public void CloseAllItemsBoughtPanel()
    {
        if (allItemsBoughtPanel != null)
            allItemsBoughtPanel.SetActive(false);
    }

    private void ShowAllItemsBoughtPanel()
    {
        if (allItemsBoughtPanel != null)
            allItemsBoughtPanel.SetActive(true);
    }

    private void RefreshAllWorkshopButtons()
    {
        WorkshopButtonController[] buttons = FindObjectsOfType<WorkshopButtonController>();

        foreach (var button in buttons)
            button.RefreshData();
    }

    private void CostTextFill()
    {
        for (int i = 0; i < costTexts.Length; i++)
        {
            if (costTexts[i] != null)
                costTexts[i].text = "";
        }

        int count = Mathf.Min(materialsNeeded.items.Length, costTexts.Length);

        for (int i = 0; i < count; i++)
        {
            var item = materialsNeeded.items[i];

            if (item == null)
            {
                Debug.LogWarning($"{name}: item {i} jest null");
                continue;
            }

            if (item.resource == null)
            {
                Debug.LogWarning($"{name}: item {i} nie ma resource");
                continue;
            }

            if (costTexts[i] == null)
            {
                Debug.LogWarning($"{name}: costTexts[{i}] nie jest przypiêty");
                continue;
            }

            string displayName = item.resource.resourceName;
            string storageName = item.resource.GetStorageName();

            int owned = ResourceManager.Instance.GetResourceCount(storageName);
            int needed = item.resourceAmount;

            costTexts[i].text = $"{displayName}: {owned}/{needed}";
        }
    }

    private void CheckIfCanBuy()
    {
        if (isBought)
        {
            canBuy = false;
            return;
        }

        canBuy = true;

        foreach (var item in materialsNeeded.items)
        {
            if (item == null || item.resource == null)
                continue;

            int owned = ResourceManager.Instance.GetResourceCount(item.resource.GetStorageName());

            if (owned < item.resourceAmount)
            {
                canBuy = false;
                return;
            }
        }
    }

    private void ApplyVisualState()
    {
        if (canBuyGlowObject != null)
            canBuyGlowObject.SetActive(!isBought && canBuy);

        if (cantBuyObject != null)
            cantBuyObject.SetActive(!isBought && !canBuy);

        if (doneObject != null)
            doneObject.SetActive(isBought);
    }

    private void CheckAllWorkshopItemsBought()
    {
        WorkshopButtonController[] buttons = FindObjectsOfType<WorkshopButtonController>();

        foreach (var button in buttons)
        {
            if (!button.isBought)
                return;
        }

        allWorkshopItemsBought = true;

        ShowAllItemsBoughtPanel();

        onAllWorkshopItemsBought?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isBought && canBuy)
        {
            RuntimeManager.PlayOneShot(hoverSound);
            transform.localScale = originalScale * 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}