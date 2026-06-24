using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlanetConfirmationUI : MonoBehaviour
{
    #region Settings

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI resourcesText;
    public TextMeshProUGUI fuelCostText;
    public Button confirmButton;
    public Button cancelButton;

    public int lowThreshold = 1;
    public int midThreshold = 15;

    #endregion

    #region State

    int currentFuelCost;

    #endregion

    #region Logic

    public void Setup(CelestialBodySettings settings, int fuelCost, Action onConfirm, Action onCancel)
    {
        if (!settings)
            return;

        currentFuelCost = fuelCost;

        questionText.text = $"Do you want to explore {settings.planetName}?";
        descriptionText.text = string.IsNullOrEmpty(settings.planetDescription) ? "No description available." : settings.planetDescription;
        fuelCostText.text = $"Fuel Cost: {currentFuelCost}";

        DisplayResources(settings.resourceConfig);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => OnConfirmClicked(onConfirm));

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            gameObject.SetActive(false);
        });
    }

    void OnConfirmClicked(Action onConfirm)
    {
        if (!ResourceManager.Instance)
            return;

        if (!ResourceManager.Instance.ConsumeResource("Fuel", currentFuelCost))
        {
            Debug.Log("Not enough fuel to travel!");

            if (FuelPopup.Instance)
                FuelPopup.Instance.ShowMessage("Not enough fuel!");

            return;
        }

        onConfirm?.Invoke();
    }

    void DisplayResources(ResourceGeneratorConfig config)
    {
        if (!config)
        {
            resourcesText.text = "No resources found";
            return;
        }

        resourcesText.text = $"Food: {GetResourceLevel(config, "Food", "Coconut", "magma fruit", "Icey fruit", "Icey", "Magma", "Fruit-Ice")}\n" +
                             $"Coal: {GetResourceLevel(config, "Coal")}\n" +
                             $"Iron: {GetResourceLevel(config, "Iron")}\n" +
                             $"Aluminium: {GetResourceLevel(config, "Aluminium")}\n" +
                             $"Titan: {GetResourceLevel(config, "Titan")}";
    }

    string GetResourceLevel(ResourceGeneratorConfig config, params string[] types)
    {
        int total = 0;

        foreach (var item in config.resources)
        {
            if (!item.prefab)
                continue;

            foreach (string type in types)
                if (item.prefab.name.Contains(type))
                    total += item.count;
        }

        if (total < lowThreshold)
            return "<color=#FF5555>Low</color>";

        if (total <= midThreshold)
            return "<color=#FFCC00>Mid</color>";

        return "<color=#55FF55>High</color>";
    }

    #endregion
}