using UnityEngine;

[CreateAssetMenu(fileName = "ResourceGeneratorConfig", menuName = "Planets/Resource Generator Config")]
public class ResourceGeneratorConfig : ScriptableObject
{
    #region Data

    public ResourceGenerator.SpawnItem[] resources;
    public ResourceGenerator.SpawnItem[] props;

    public float collisionCheckRadius = 1.0f;
    public Vector3 rotationAdjustment = Vector3.zero;
    public float waterMargin = 0.05f;
    public float oceanResolveTimeout = 1f;
    public int maxAttemptsPerInstance = 64;

    #endregion


    #region Validation

    void OnValidate()
    {
        InitDefaults(resources);
        InitDefaults(props);
    }

    void InitDefaults(ResourceGenerator.SpawnItem[] items)
    {
        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
            if (!items[i].initialized)
            {
                items[i].avoidWater = true;
                items[i].initialized = true;
            }
    }

    #endregion
}