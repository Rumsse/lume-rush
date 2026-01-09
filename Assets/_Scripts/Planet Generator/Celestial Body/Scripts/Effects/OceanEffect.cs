using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanEffect : PostProcessingEffect
{
    Light light;

    public void Update(CelestialBodyGenerator generator, Shader shader)
    {
        if (material == null || material.shader != shader)
            material = new Material(shader);

        if (light == null)
        {
            var sunCaster = GameObject.FindObjectOfType<SunShadowCaster>();
            if (sunCaster != null)
                light = sunCaster.GetComponent<Light>();
            else
                light = GameObject.FindObjectOfType<Light>();
        }

        Vector3 centre = generator.transform.position;
        float radius = generator.GetOceanRadius();
        material.SetVector("oceanCentre", centre);
        material.SetFloat("oceanRadius", radius);

        material.SetFloat("planetScale", generator.BodyScale);

        if (light != null)
        {
            // upewnij się, że przekazujemy normalizowany wektor kierunku do słońca
            Vector3 dir = -light.transform.forward;
            dir.Normalize();
            material.SetVector("dirToSun", dir);
        }

        // zapewnij kamerze depth texture (używane przez shader)
        if (Camera.main != null)
            Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        generator.body.shading.SetOceanProperties(material);
    }

    public override Material GetMaterial() => material;
}