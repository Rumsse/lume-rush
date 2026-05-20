using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WorkshopButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite unavailableImage;
    public Sprite availableImage;
    public Sprite boughtImage;

    public GameObject materialNeed;

    [SerializeField]
    private Dictionary<string, int> buyCost;

    private bool canBuy = true;
    private bool isBought = false;

    public ScriptableObject materialsNeeded;

    #region SO STRUCT

    [System.Serializable]
    public class NewDict
    {
        [SerializeField] public NewDictItem[] newDictItems;

        public Dictionary<string, int> ToDictionary()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            foreach (var item in newDictItems)
            {
                dict[item.resourceName] = item.resourceAmount;
            }

            return dict;
        }
    }

    [System.Serializable]
    public class NewDictItem
    {
        public string resourceName;
        public int resourceAmount;
    }

    #endregion

    private void OnEnable()
    {
        RefreshData();
    }
    public void BuyItem()
    {
        CheckIfCanBuy();
        if (!canBuy) return;

        BuyCostSO buyCostSO = materialsNeeded as BuyCostSO;
        var costDict = buyCostSO.newDict.ToDictionary();

        foreach (var cost in costDict)
        {
            ResourceStorage.Instance.Spend(cost.Key, cost.Value);
        }

        Debug.Log("Purchased item!");

        isBought = true;

        GetComponent<Image>().sprite = boughtImage;
        GetComponent<Button>().interactable = false;

        RefreshData();
    }

    public void CostTextFill()
    {
        var textList = materialNeed.GetComponentsInChildren<TextMeshProUGUI>();

        int i = 0;

        foreach (var cost in buyCost)
        {
            ResourceStorage.Instance.resources.TryGetValue(cost.Key, out int owned);

            textList[i].text = $"{cost.Key}: {owned}/{cost.Value}";
            i++;
        }

        for (int j = i; j < textList.Length; j++)
        {
            textList[j].text = "";
        }
    }

    public void CheckIfCanBuy()
    {
        canBuy = true;

        foreach (var cost in buyCost)
        {
            if (!ResourceStorage.Instance.HasEnough(cost.Key, cost.Value))
            {
                canBuy = false;
                GetComponent<Image>().sprite = unavailableImage;
                return;
            }
        }

        GetComponent<Image>().sprite = availableImage;
    }
    private void RefreshData()
    {
        BuyCostSO buyCostSO = materialsNeeded as BuyCostSO;
        buyCost = buyCostSO.newDict.ToDictionary();

        CostTextFill();
        CheckIfCanBuy();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isBought && canBuy)
            transform.localScale *= 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isBought && canBuy)
            transform.localScale /= 1.05f;
    }
}