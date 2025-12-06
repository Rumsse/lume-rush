using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PlanetSpawnManager : MonoBehaviour
{
    [SerializeField] PlanetSpawnSettings settings;
    [SerializeField]
    bool randomizeSeedEverySpawn = true;

    #region Unity

    void Awake() => settings = settings != null ? settings : CreateDefaultSettings();

    void Start()
    {
        if (settings.spawnOnStart)
            SpawnPlanet();
    }

    #endregion

    #region API

    public GameObject SpawnPlanet() => SpawnPlanet(settings.spawnPosition, Quaternion.Euler(settings.spawnEuler), settings.parent, null);

    public GameObject SpawnPlanet(int forcedSeed) => SpawnPlanet(settings.spawnPosition, Quaternion.Euler(settings.spawnEuler), settings.parent, forcedSeed);

    public GameObject SpawnPlanet(Vector3 position, Quaternion rotation, Transform parent = null, int? forcedSeed = null)
    {
        if (settings == null || settings.planetPrefab == null)
            return null;

        var go = Instantiate(settings.planetPrefab, position, rotation, parent);
        var seed = forcedSeed ?? (randomizeSeedEverySpawn ? UnityEngine.Random.Range(settings.seedMin, settings.seedMax) : (int?)null);
        if (seed.HasValue)
            ApplySeedToShapeScriptables(go, seed.Value);
        return go;
    }

    #endregion

    #region Internal reflection helpers

    void ApplySeedToShapeScriptables(GameObject root, int seed)
    {
        var comps = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var comp in comps)
        {
            if (comp == null) // missing script
                continue;

            var t = comp.GetType();
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                if (!typeof(ScriptableObject).IsAssignableFrom(f.FieldType))
                    continue;

                var original = f.GetValue(comp) as ScriptableObject;
                if (original == null)
                    continue;

                if (!FieldOrTypeNameSuggestsShape(f))
                    continue;

                var clone = Instantiate(original);
                TrySetSeed(clone, seed);
                f.SetValue(comp, clone);
            }

            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(p => p.CanWrite && typeof(ScriptableObject).IsAssignableFrom(p.PropertyType));
            foreach (var p in props)
            {
                if (!PropertyOrTypeNameSuggestsShape(p))
                    continue;

                var original = p.GetValue(comp) as ScriptableObject;
                if (original == null)
                    continue;

                var clone = Instantiate(original);
                TrySetSeed(clone, seed);
                p.SetValue(comp, clone);
            }
        }
    }

    static bool FieldOrTypeNameSuggestsShape(FieldInfo f)
    {
        var n = f.Name.ToLowerInvariant();
        var type = f.FieldType.Name.ToLowerInvariant();
        return n.Contains("shape") || n.Contains("shapeSettings") || type.Contains("shape") || type.Contains("shapesettings");
    }

    static bool PropertyOrTypeNameSuggestsShape(PropertyInfo p)
    {
        var n = p.Name.ToLowerInvariant();
        var type = p.PropertyType.Name.ToLowerInvariant();
        return n.Contains("shape") || n.Contains("shapeSettings") || type.Contains("shape") || type.Contains("shapesettings");
    }

    static void TrySetSeed(ScriptableObject so, int seed)
    {
        var t = so.GetType();
        var f = t.GetField("seed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && (f.FieldType == typeof(int) || f.FieldType == typeof(long)))
        {
            if (f.FieldType == typeof(int))
                f.SetValue(so, seed);
            else
                f.SetValue(so, (long)seed);
            return;
        }

        var p = t.GetProperty("seed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite && (p.PropertyType == typeof(int) || p.PropertyType == typeof(long)))
        {
            if (p.PropertyType == typeof(int))
                p.SetValue(so, seed);
            else
                p.SetValue(so, (long)seed);
        }
    }

    #endregion

    PlanetSpawnSettings CreateDefaultSettings()
    {
        var s = ScriptableObject.CreateInstance<PlanetSpawnSettings>();
        s.spawnOnStart = false;
        return s;
    }
}