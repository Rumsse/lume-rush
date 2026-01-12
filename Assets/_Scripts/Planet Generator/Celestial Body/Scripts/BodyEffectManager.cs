using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BodyEffectManager : MonoBehaviour
{

    public static BodyEffectManager Instance;

    public Shader oceanShader;
    public bool displayOceans = true;

    List<CelestialBodyGenerator> generators;
    OceanEffect[] oceanEffects;
    List<Material> postProcessingMaterials = new List<Material>();

    // Flaga, żeby nie spamować konsoli tysiącem logów na sekundę
    private bool debugLoggedOnce = false;

    void OnEnable()
    {
        Instance = this;
        Debug.Log("🟢 [BodyEffectManager] OnEnable - Manager włączony!");
        Init();
    }

    void Init()
    {
        if (generators == null || generators.Count == 0)
        {
            generators = new List<CelestialBodyGenerator>(FindObjectsOfType<CelestialBodyGenerator>());
            Debug.Log($"🔍 [BodyEffectManager] Znaleziono {generators.Count} planet.");
        }

        if (displayOceans && (oceanEffects == null || oceanEffects.Length != generators.Count))
        {
            oceanEffects = new OceanEffect[generators.Count];
        }
    }

    void Update()
    {
        // Logika debugowania - wykona się tylko raz
        if (!debugLoggedOnce)
        {
            if (oceanShader == null) Debug.LogError("🔴 [BodyEffectManager] BRAK SHADERA! Przypisz plik Ocean_URP w Inspectorze.");
            else Debug.Log("🟢 [BodyEffectManager] Shader jest przypisany.");

            if (generators.Count == 0) Debug.LogWarning("🟠 [BodyEffectManager] Nie widzę żadnych planet (CelestialBodyGenerator).");

            debugLoggedOnce = true;
        }

        PrepareMaterials();
    }

    void PrepareMaterials()
    {
        if (oceanShader == null) return;

        Init();

        // Czyścimy listę
        postProcessingMaterials.Clear();

        for (int i = 0; i < generators.Count; i++)
        {
            var generator = generators[i];
            if (generator == null) continue;

            if (displayOceans && generator.body.shading.hasOcean)
            {
                if (oceanEffects[i] == null) oceanEffects[i] = new OceanEffect();

                // To jest kluczowy moment
                oceanEffects[i].Update(generator, oceanShader);

                Material mat = oceanEffects[i].GetMaterial();
                if (mat != null)
                {
                    postProcessingMaterials.Add(mat);
                    // Debug.Log($"🌊 Dodano materiał oceanu dla planety: {generator.name}"); // Odkomentuj jeśli chcesz spam
                }
            }
        }
    }

    public List<Material> GetMaterials()
    {
        // Sprawdzamy czy Renderer Feature w ogóle o to pyta
        // Debug.Log("Renderer Feature pobiera materiały..."); 
        return postProcessingMaterials;
    }
}