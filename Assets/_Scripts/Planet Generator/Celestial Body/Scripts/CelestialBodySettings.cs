using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Celestial Body/Settings Holder")]
public class CelestialBodySettings : ScriptableObject
{
    public string planetName;
    public CelestialBodyShape shape;
    public CelestialBodyShading shading;
    public ResourceGeneratorConfig resourceConfig;

    public string planetDescription;
    public Material planetMaterial;

    [Header("Gravity Settings")]
    [Min(0f)] public float gravityForce = 800f;
}