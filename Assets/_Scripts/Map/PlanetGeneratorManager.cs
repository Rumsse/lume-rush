using System.Collections.Generic;
using UnityEngine;

public class PlanetGeneratorManager : MonoBehaviour
{
    public List<CelestialBodySettings> planetPresets;

    void Start()
    {
        string selectedPlanetName = PlayerPrefs.GetString("SelectedPlanet", "Default");
        var selectedPreset = planetPresets.Find(p => p.name == selectedPlanetName);

        if (selectedPreset)
        {
            var spawner = GetComponent<PlanetSpawnerManager>();
            spawner.presets = new List<CelestialBodySettings> { selectedPreset };
            spawner.spawnCount = 1;
            spawner.SpawnAll();
        }
        else
        {
            Debug.LogWarning("Selected planet preset not found!");
        }
    }
}