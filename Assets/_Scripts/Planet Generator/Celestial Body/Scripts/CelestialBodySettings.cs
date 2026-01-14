using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Settings Holder")]
public class CelestialBodySettings : ScriptableObject
{
    public CelestialBodyShape shape;
    public CelestialBodyShading shading;
    public ResourceGeneratorConfig resourceConfig;
}