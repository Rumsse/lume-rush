using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlanetUI : MonoBehaviour
{
    public TextMeshProUGUI planetNameText;
    public Button selectPlanetButton;

    private CelestialBodySettings preset;

    public void Setup(CelestialBodySettings bodyPreset, System.Action onPlanetSelected)
    {
        preset = bodyPreset;
        planetNameText.text = preset.name;

        selectPlanetButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("SelectedPlanetPreset", preset.name);
            PlayerPrefs.Save();

            onPlanetSelected?.Invoke();
        });
    }
}