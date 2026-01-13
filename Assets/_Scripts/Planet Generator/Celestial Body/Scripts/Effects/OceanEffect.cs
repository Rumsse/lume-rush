using UnityEngine;

public class OceanEffect : PostProcessingEffect
{
    Light light;

    public static Material CurrentOceanMaterial;

    public void Update(CelestialBodyGenerator generator, Shader shader)
    {
        if (material == null || material.shader != shader)
            material = new Material(shader);

        CurrentOceanMaterial = material;

        if (light == null)
        {
            var lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
                if (l.type == LightType.Directional) { light = l; break; }

            /*var sunCaster = GameObject.FindObjectOfType<SunShadowCaster>();
            if (sunCaster != null)
                light = sunCaster.GetComponent<Light>();*/
            //else
            //{
            // Fallback na pierwsze lepsze światło kierunkowe

            //}
        }

        Vector3 centre = generator.transform.position;
        float radius = generator.GetOceanRadius();
        material.SetVector("oceanCentre", centre);
        material.SetFloat("oceanRadius", radius);
        material.SetFloat("planetScale", generator.BodyScale);

        if (light != null)
        {
            Vector3 dir = -light.transform.forward;
            material.SetVector("dirToSun", dir.normalized);
        }

        generator.body.shading.SetOceanProperties(material);
    }

    public override Material GetMaterial() => material;
}