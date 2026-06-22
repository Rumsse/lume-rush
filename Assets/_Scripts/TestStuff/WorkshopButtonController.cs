using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FMODUnity;

public class WorkshopButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite unavailableImage;
    public Sprite availableImage;
    public Sprite boughtImage;

    public BuyCostSO materialsNeeded;
    public TextMeshProUGUI[] costTexts;

    public EventReference hoverSound;
    public EventReference buySound;
    public EventReference notEnoughMaterialsSound;

    private bool canBuy;
    private bool isBought;

    private void OnEnable()
    {
        StartCoroutine(WaitForResourceManagerAndRefresh());
    }

    private IEnumerator WaitForResourceManagerAndRefresh()
    {
        while (ResourceManager.Instance == null)
            yield return null;

        RefreshData();
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

        foreach (var item in materialsNeeded.items)
        {
            if (item.resource == null)
                continue;

            ResourceManager.Instance.ConsumeResource(item.resource.GetStorageName(), item.resourceAmount);
        }

        RuntimeManager.PlayOneShot(buySound);

        isBought = true;

        GetComponent<Image>().sprite = boughtImage;
        GetComponent<Button>().interactable = false;

        RefreshAllWorkshopButtons();
    }

    public void RefreshData()
    {
        if (ResourceManager.Instance == null || materialsNeeded == null || materialsNeeded.items == null)
            return;

        CostTextFill();
        CheckIfCanBuy();
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

            if (item == null || item.resource == null || costTexts[i] == null)
                continue;

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
            GetComponent<Image>().sprite = boughtImage;
            return;
        }

        canBuy = true;

        foreach (var item in materialsNeeded.items)
        {
            if (item == null || item.resource == null)
                continue;

            if (ResourceManager.Instance.GetResourceCount(item.resource.GetStorageName()) < item.resourceAmount)
            {
                canBuy = false;
                GetComponent<Image>().sprite = unavailableImage;
                return;
            }
        }

        GetComponent<Image>().sprite = availableImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isBought && canBuy)
        {
            RuntimeManager.PlayOneShot(hoverSound);
            transform.localScale *= 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isBought && canBuy)
            transform.localScale /= 1.05f;
    }
}