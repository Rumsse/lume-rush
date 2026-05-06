using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetUI : MonoBehaviour
{
    // chyba stary kod 

    public TextMeshProUGUI planetNameText;
    public Button selectPlanetButton;
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationText;
    public Button confirmButton;
    public Button cancelButton;

    private CelestialBodySettings preset;

    public void Setup(CelestialBodySettings bodyPreset, System.Action onConfirmedAction)
    {
        preset = bodyPreset;
        planetNameText.text = preset.name;

        selectPlanetButton.onClick.AddListener(() =>
        {
            ShowConfirmationPanel(onConfirmedAction);
        });
    }

    private void ShowConfirmationPanel(System.Action onConfirmedAction)
    {
        confirmationPanel.SetActive(true);
        confirmationText.text = $"Czy udaæ siê na planetê {preset.name}?";

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(false);
        });

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(false);

            PlayerPrefs.SetString("SelectedPlanetPreset", preset.name);
            PlayerPrefs.Save();
            onConfirmedAction?.Invoke();
        });
    }
}