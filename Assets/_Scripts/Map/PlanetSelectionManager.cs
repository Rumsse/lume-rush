using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetSelectionManager : MonoBehaviour
{
    #region Map Generation Settings

    public GameObject starPrefab; // Prefab gwiazdy (œrodek uk³adu)
    public GameObject planetPrefab; // Prefab fizyczny planety w uk³adzie
    public GameObject planetUIPrefab; // Prefab przycisku w interfejsie
    public Transform starParent; // Rodzic dla gwiazdy
    public RectTransform planetsParentUI; // Kontener dla UI przycisków
    public Vector2 orbitRange = new Vector2(50f, 200f); // Minimalne i maksymalne odleg³oœci orbit

    public CelestialBodySettings mainPlanetPreset; // G³ówna planeta, np. "pla¿a"
    public List<CelestialBodySettings> otherPlanetPresets; // Pozosta³e planety
    public int planetCount = 5;
    public float uiButtonSpacing = 100f; // Odstêp miêdzy przyciskami UI

    private List<PlanetData> generatedPlanets = new List<PlanetData>();

    #endregion

    void Start() => GeneratePlanetMap();

    /// <summary>
    /// Generuj mapê planet (gwiazda + planety orbitalne logicznie rozmieszczone wzglêdem gwiazdy).
    /// </summary>
    void GeneratePlanetMap()
    {
        // Dodaj gwiazdê (œrodek mapy uk³adu)
        if (starPrefab)
        {
            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, starParent);
            star.name = "Star_Center";
        }

        // Dodaj g³ówn¹ planetê (pierwsz¹, np. "pla¿a")
        if (mainPlanetPreset)
            AddPlanet(mainPlanetPreset, GetOrbitPosition(0), true);

        // Dodaj inne losowe planety
        var availablePresets = new List<CelestialBodySettings>(otherPlanetPresets);
        for (int i = 1; i < planetCount; i++)
        {
            if (availablePresets.Count == 0) break;

            // Losowo wybierz preset z pozosta³ych
            var randomPreset = availablePresets[Random.Range(0, availablePresets.Count)];
            availablePresets.Remove(randomPreset);

            // Dodaj planetê na nowej pozycji orbitalnej
            Vector3 orbitPosition = GetOrbitPosition(i);
            AddPlanet(randomPreset, orbitPosition, false);
        }
    }

    /// <summary>
    /// Dodaj planetê i jej odpowiedni przycisk w UI.
    /// </summary>
    void AddPlanet(CelestialBodySettings preset, Vector3 orbitPosition, bool isFirst)
    {
        // Nazwa logiczna planety
        string planetName = isFirst ? "MainPlanet" : $"Planet_{generatedPlanets.Count + 1}";

        // Dodaj dane planety
        var planetData = new PlanetData(preset, orbitPosition, planetName);
        generatedPlanets.Add(planetData);

        // Stwórz fizyczny prefab planety w uk³adzie
        var planetObj = Instantiate(planetPrefab, orbitPosition, Quaternion.identity, starParent);
        planetObj.name = planetName;

        // Opcjonalne: ustaw wielkoœæ planety na podstawie presetu (jeœli u¿ywasz np. skali)
        float randomScale = Random.Range(1f, 3f);
        planetObj.transform.localScale = Vector3.one * randomScale;

        // Stwórz przycisk w UI z odstêpem (offsetem)
        var planetUI = Instantiate(planetUIPrefab, planetsParentUI);
        Vector2 buttonPosition = new Vector2(0, -uiButtonSpacing * (generatedPlanets.Count - 1)); // Offset w osi Y
        planetUI.GetComponent<RectTransform>().anchoredPosition = buttonPosition;

        // Skonfiguruj zachowanie przycisku
        planetUI.GetComponent<PlanetUI>().Setup(preset, () => OnPlanetSelected(planetData));
    }

    /// <summary>
    /// Pozycja planety w orbicie (symulacja logicznej orbity).
    /// </summary>
    /// <param name="index">Indeks orbity</param>
    /// <returns>Pozycja w przestrzeni w obrêbie orbity</returns>
    Vector3 GetOrbitPosition(int index)
    {
        // Losowa odleg³oœæ od gwiazdy (w zakresie orbit)
        float distance = Mathf.Lerp(orbitRange.x, orbitRange.y, (float)index / planetCount);

        // Losowy k¹t w p³aszczyŸnie (symulacja orbity ko³owej)
        float angle = Random.Range(0f, 360f);
        float radians = angle * Mathf.Deg2Rad;

        // Zwracaj pozycjê w lokalnym uk³adzie wspó³rzêdnych (orbita planeta wokó³ gwiazdy)
        return new Vector3(
            Mathf.Cos(radians) * distance,
            Random.Range(-5f, 5f), // Minimalne nachylenie orbity
            Mathf.Sin(radians) * distance
        );
    }

    /// <summary>
    /// Funkcja wywo³ywana po klikniêciu przycisku planety.
    /// Zapisuje wybran¹ planetê i przechodzi do nowej sceny.
    /// </summary>
    void OnPlanetSelected(PlanetData planetData)
    {
        PlayerPrefs.SetString("SelectedPlanet", planetData.Preset.name);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Test Planet Exploration");
    }
}

[System.Serializable]
public class PlanetData
{
    public CelestialBodySettings Preset;
    public Vector3 Position;
    public string Name;

    public PlanetData(CelestialBodySettings preset, Vector3 position, string name)
    {
        Preset = preset;
        Position = position;
        Name = name;
    }
}