using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    public static ResourceUI Instance { get; private set; }

    [System.Serializable]
    public class ResourceDisplay
    {
        public string resourceName;
        public Image icon;
        public TextMeshProUGUI countText;
        public GameObject container;
    }

    [SerializeField] List<ResourceDisplay> resourceDisplays = new List<ResourceDisplay>();

    void Awake() => Instance = this;

    void Start() => UpdateUI();

    void OnEnable() => UpdateUI();

    public void UpdateUI()
    {
        if (SpaceShipStorage.Instance == null)
            return;

        foreach (var display in resourceDisplays)
        {
            if (display == null || string.IsNullOrEmpty(display.resourceName))
                continue;

            int count = SpaceShipStorage.Instance.GetResourcesCount(display.resourceName);

            if (display.countText != null)
                display.countText.text = count.ToString();

            if (display.container != null)
                display.container.SetActive(count > 0);
        }
    }
}