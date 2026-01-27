using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetSelectionManager : MonoBehaviour
{
    #region Map Generation Settings

    public GameObject starPrefab;
    public GameObject planetUIPrefab;
    public Transform starParent;
    public RectTransform planetsParentUI;

    public CelestialBodySettings mainPlanetPreset;
    public List<CelestialBodySettings> otherPlanetPresets;
    public int planetCount = 5;
    public float minOrbitDistance = 50f;
    public float maxOrbitDistance = 200f;

    private List<PlanetData> generatedPlanets = new List<PlanetData>();

    #endregion

    void Start() => GeneratePlanetMap();

    void GeneratePlanetMap()
    {
        if (starPrefab)
        {
            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, starParent);
            star.name = "Star_Center";
        }

        if (mainPlanetPreset)
            AddPlanet(mainPlanetPreset, Vector3.right * Random.Range(minOrbitDistance, maxOrbitDistance), true);

        var availablePresets = new List<CelestialBodySettings>(otherPlanetPresets);

        for (int i = 1; i < planetCount; i++)
        {
            if (availablePresets.Count == 0) break;

            var randomPreset = availablePresets[Random.Range(0, availablePresets.Count)];
            availablePresets.Remove(randomPreset);

            Vector3 randomPosition = Random.onUnitSphere * Random.Range(minOrbitDistance, maxOrbitDistance);
            AddPlanet(randomPreset, randomPosition, false);
        }
    }

    void AddPlanet(CelestialBodySettings preset, Vector3 position, bool isFirst)
    {
        string planetName = isFirst ? "MainPlanet" : $"Planet_{generatedPlanets.Count + 1}";
        var planetData = new PlanetData(preset, position, planetName);
        generatedPlanets.Add(planetData);

        var planetUI = Instantiate(planetUIPrefab, planetsParentUI);
        planetUI.GetComponent<PlanetUI>().Setup(preset, () => OnPlanetSelected(planetData));
    }

    void OnPlanetSelected(PlanetData planetData)
    {
        PlayerPrefs.SetString("SelectedPlanet", planetData.Name);
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