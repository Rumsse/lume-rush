using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanetSelectionManager : MonoBehaviour
{
    public GameObject starPrefab;
    public GameObject planetPrefab;
    public Transform starParent;
    public Vector2 orbitRange = new Vector2(50f, 200f);

    public CelestialBodySettings mainPlanetPreset;
    public List<CelestialBodySettings> otherPlanetPresets;
    public int planetCount = 5;

    List<PlanetData> generatedPlanets = new List<PlanetData>();

    void Start() => GeneratePlanetMap();

    void GeneratePlanetMap()
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
    }

    void AddPlanet(CelestialBodySettings preset, Vector3 orbitPosition, bool isFirst)
    {
        var planetObj = Instantiate(planetPrefab, orbitPosition, Quaternion.identity, starParent);

        var chosenName = string.IsNullOrWhiteSpace(preset.planetName)
            ? isFirst ? "MainPlanet" : $"Planet_{generatedPlanets.Count + 1:00}"
            : preset.planetName;

        planetObj.name = chosenName;

        var interactable = planetObj.AddComponent<PlanetInteractable>();
        interactable.spawnPanelPosition = planetObj.transform;
        interactable.PlanetName = chosenName;
        interactable.SelectedPresetName = preset.name;

        generatedPlanets.Add(new PlanetData(preset, orbitPosition, planetObj.name));
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