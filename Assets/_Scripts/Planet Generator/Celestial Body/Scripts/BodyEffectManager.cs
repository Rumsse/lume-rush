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

    void OnEnable()
    {
        Instance = this;
        Init();
    }

    void OnDisable()
    {
        OceanEffect.CurrentOceanMaterial = null;
        postProcessingMaterials.Clear();
    }

    void Init()
    {
        if (generators == null || generators.Count == 0)
            generators = new List<CelestialBodyGenerator>(FindObjectsByType<CelestialBodyGenerator>(FindObjectsSortMode.None));

        if (displayOceans && (oceanEffects == null || oceanEffects.Length != generators.Count))
            oceanEffects = new OceanEffect[generators.Count];
    }

    void Update()
    {
        PrepareMaterials();
    }

    void PrepareMaterials()
    {
        if (oceanShader == null)
        {
            OceanEffect.CurrentOceanMaterial = null;
            return;
        }

        Init();

        postProcessingMaterials.Clear();
        bool hasActiveOcean = false;

        for (int i = 0; i < generators.Count; i++)
        {
            var generator = generators[i];
            if (generator == null)
                continue;

            if (displayOceans && generator.body.shading.hasOcean)
            {
                if (oceanEffects[i] == null)
                    oceanEffects[i] = new OceanEffect();

                oceanEffects[i].Update(generator, oceanShader);

                Material mat = oceanEffects[i].GetMaterial();
                if (mat != null)
                {
                    postProcessingMaterials.Add(mat);
                    hasActiveOcean = true;
                }
            }
        }

        if (!hasActiveOcean)
            OceanEffect.CurrentOceanMaterial = null;
    }

    public List<Material> GetMaterials() => postProcessingMaterials;
}