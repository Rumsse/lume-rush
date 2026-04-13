using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlanetConfirmationUI : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI resourcesText;
    public Button confirmButton;
    public Button cancelButton;

    public int lowThreshold = 1;
    public int midThreshold = 15;

    public void Setup(CelestialBodySettings settings, System.Action onConfirm, System.Action onCancel)
    {
        if (!settings)
            return;

        questionText.text = $"Do you want to explore {settings.planetName}?";
        DisplayResources(settings.resourceConfig);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => onConfirm?.Invoke());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            gameObject.SetActive(false);
        });
    }

    void DisplayResources(ResourceGeneratorConfig config)
    {
        if (!config)
        {
            resourcesText.text = "No resources found";
            return;
        }

        resourcesText.text = $"Food: {GetResourceLevel(config, "Food")}\n" +
                             $"Coal: {GetResourceLevel(config, "Coal")}\n" +
                             $"Iron: {GetResourceLevel(config, "Iron")}\n" +
                             $"Aluminium: {GetResourceLevel(config, "Aluminium")}\n" +
                             $"Titan: {GetResourceLevel(config, "Titan")}";
    }

    string GetResourceLevel(ResourceGeneratorConfig config, string type)
    {
        int total = 0;

        foreach (var item in config.resources)
            if (item.prefab && item.prefab.name.Contains(type))
                total += item.count;

        if (total < lowThreshold)
            return "<color=#FF5555>Low</color>";

        if (total <= midThreshold)
            return "<color=#FFCC00>Mid</color>";

        return "<color=#55FF55>High</color>";
    }
}