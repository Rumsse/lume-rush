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


    void OnEnable()
    {
        Instance = this;
        Init();
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
        if (oceanShader == null) return;

        Init();

        postProcessingMaterials.Clear();

        for (int i = 0; i < generators.Count; i++)
        {
            var generator = generators[i];
            if (generator == null) continue;

            if (displayOceans && generator.body.shading.hasOcean)
            {
                if (oceanEffects[i] == null) oceanEffects[i] = new OceanEffect();

                oceanEffects[i].Update(generator, oceanShader);

                Material mat = oceanEffects[i].GetMaterial();
                if (mat != null)
                {
                    postProcessingMaterials.Add(mat);
                }
            }
        }
    }

    public List<Material> GetMaterials()
    {
        return postProcessingMaterials;
    }
}