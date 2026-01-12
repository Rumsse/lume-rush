using UnityEngine;

public class OceanEffect : PostProcessingEffect
{
    Light light;

    // Dodajemy statyczną referencję, aby Renderer Feature miał dostęp do materiału
    public static Material CurrentOceanMaterial;

    public void Update(CelestialBodyGenerator generator, Shader shader)
    {
        if (material == null || material.shader != shader)
            material = new Material(shader);

        // Przypisz materiał do zmiennej statycznej
        CurrentOceanMaterial = material;

        // Debug: sprawdź czy materiał faktycznie został przypisany
        if (CurrentOceanMaterial == null)
        {
            Debug.LogError("[OceanEffect] CurrentOceanMaterial nie zostało przypisane (null).", generator.gameObject);
            Debug.Assert(false, "[OceanEffect] CurrentOceanMaterial jest null po przypisaniu.");
        }
        else
        {
            Debug.Log($"[OceanEffect] CurrentOceanMaterial przypisany: {CurrentOceanMaterial.name}", generator.gameObject);
        }

        if (light == null)
        {
            var sunCaster = GameObject.FindObjectOfType<SunShadowCaster>();
            if (sunCaster != null)
                light = sunCaster.GetComponent<Light>();
            else
            {
                // Fallback na pierwsze lepsze światło kierunkowe
                var lights = GameObject.FindObjectsOfType<Light>();
                foreach (var l in lights)
                    if (l.type == LightType.Directional) { light = l; break; }
            }
        }

        Vector3 centre = generator.transform.position;
        float radius = generator.GetOceanRadius();
        material.SetVector("oceanCentre", centre);
        material.SetFloat("oceanRadius", radius);
        material.SetFloat("planetScale", generator.BodyScale);

        if (light != null)
        {
            Vector3 dir = -light.transform.forward;
            // Normalizacja jest ważna
            material.SetVector("dirToSun", dir.normalized);
        }

        // Ta linijka w URP jest zbędna, usuwamy ją:
        // if (Camera.main != null) Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        generator.body.shading.SetOceanProperties(material);
    }

    public override Material GetMaterial() => material;
}