using System.Collections.Generic;
using UnityEngine;

public class PlanetSelectionManager : MonoBehaviour
{
    #region Settings

    public GameObject starPrefab;
    public GameObject planetPrefab;
    public Transform starParent;
    public Vector2 orbitRange = new Vector2(50f, 200f);

    public CelestialBodySettings mainPlanetPreset;
    public List<CelestialBodySettings> otherPlanetPresets;
    public int planetCount = 5;

    #endregion

    #region State

    static List<PlanetData> savedPlanets;

    List<PlanetData> generatedPlanets = new List<PlanetData>();

    #endregion

    #region Unity Callbacks

    void Start()
    {
        if (savedPlanets == null)
            GenerateNewMap();
        else
            LoadSavedMap();
    }

    #endregion

    #region Logic

    public static void ResetMapData() => savedPlanets = null;

    void GenerateNewMap()
    {
        if (starPrefab)
        {
            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, starParent);
            star.name = "Star_Center";
        }

        if (mainPlanetPreset)
            AddPlanet(mainPlanetPreset, GetOrbitPosition(0), true);

        var availablePresets = new List<CelestialBodySettings>(otherPlanetPresets);
        for (int i = 1; i < planetCount; i++)
        {
            if (availablePresets.Count == 0) break;

            var randomPreset = availablePresets[Random.Range(0, availablePresets.Count)];
            availablePresets.Remove(randomPreset);

            var orbitPosition = GetOrbitPosition(i);
            AddPlanet(randomPreset, orbitPosition, false);
        }

        savedPlanets = new List<PlanetData>(generatedPlanets);
    }

    void LoadSavedMap()
    {
        generatedPlanets = savedPlanets;

        if (starPrefab)
        {
            var star = Instantiate(starPrefab, Vector3.zero, Quaternion.identity, starParent);
            star.name = "Star_Center";
        }

        foreach (var data in generatedPlanets)
            RebuildPlanet(data);
    }

    void AddPlanet(CelestialBodySettings preset, Vector3 orbitPosition, bool isFirst)
    {
        var chosenName = string.IsNullOrWhiteSpace(preset.planetName)
            ? isFirst ? "MainPlanet" : $"Planet_{generatedPlanets.Count + 1:00}"
            : preset.planetName;

        var data = new PlanetData(preset, orbitPosition, chosenName);
        generatedPlanets.Add(data);

        RebuildPlanet(data);
    }

    void RebuildPlanet(PlanetData data)
    {
        var planetObj = Instantiate(planetPrefab, data.Position, Quaternion.identity, starParent);
        planetObj.name = data.Name;

        var interactable = planetObj.AddComponent<PlanetInteractable>();
        interactable.spawnPanelPosition = planetObj.transform;

        interactable.planetSettings = data.Preset;
    }

    Vector3 GetOrbitPosition(int index)
    {
        var distance = Mathf.Lerp(orbitRange.x, orbitRange.y, (float)index / planetCount);
        var radians = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Cos(radians) * distance,
            Random.Range(-5f, 5f),
            Mathf.Sin(radians) * distance
        );
    }

    #endregion

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
}