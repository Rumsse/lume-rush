using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetSpawnerManager))]
public class PlanetSpawnerManagerEditor : Editor
{
    SerializedProperty presets;
    SerializedProperty spawnCount;
    SerializedProperty spawnRadius;
    SerializedProperty scaleRange;
    SerializedProperty randomizeSeed;
    SerializedProperty seedMin;
    SerializedProperty seedMax;
    SerializedProperty parentUnderThis;

    SerializedProperty addGravityArea;
    SerializedProperty gravityAreaMultiplier;
    SerializedProperty gravityAreaIsTrigger;
    SerializedProperty gravityComponentTypeName;
    SerializedProperty planetTag;
    SerializedProperty planetLayerName;

    bool spawnFoldout = true;
    bool gravityFoldout = true;

    void OnEnable()
    {
        presets = serializedObject.FindProperty("presets");
        spawnCount = serializedObject.FindProperty("spawnCount");
        spawnRadius = serializedObject.FindProperty("spawnRadius");
        scaleRange = serializedObject.FindProperty("scaleRange");
        randomizeSeed = serializedObject.FindProperty("randomizeSeed");
        seedMin = serializedObject.FindProperty("seedMin");
        seedMax = serializedObject.FindProperty("seedMax");
        parentUnderThis = serializedObject.FindProperty("parentUnderThis");

        addGravityArea = serializedObject.FindProperty("addGravityArea");
        gravityAreaMultiplier = serializedObject.FindProperty("gravityAreaMultiplier");
        gravityAreaIsTrigger = serializedObject.FindProperty("gravityAreaIsTrigger");
        gravityComponentTypeName = serializedObject.FindProperty("gravityComponentTypeName");
        planetTag = serializedObject.FindProperty("planetTag");
        planetLayerName = serializedObject.FindProperty("planetLayerName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Planet Spawner Manager", EditorStyles.boldLabel);

        DrawPresetsSection();
        DrawSpawnSection();
        DrawGravitySection();
        DrawTagLayerSection();
        DrawActionsSection();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawPresetsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(presets, true);

        if (presets.arraySize == 0)
            EditorGUILayout.HelpBox("Add at least 1 CelestialBodySettings preset.", MessageType.Warning);
    }

    void DrawSpawnSection()
    {
        spawnFoldout = EditorGUILayout.Foldout(spawnFoldout, "Spawning", true);
        if (!spawnFoldout) return;

        EditorGUILayout.PropertyField(spawnCount);
        EditorGUILayout.PropertyField(spawnRadius);
        EditorGUILayout.PropertyField(scaleRange);
        EditorGUILayout.PropertyField(parentUnderThis);

        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(randomizeSeed);
        if (randomizeSeed.boolValue)
        {
            EditorGUILayout.PropertyField(seedMin);
            EditorGUILayout.PropertyField(seedMax);

            if (seedMin.intValue >= seedMax.intValue)
                EditorGUILayout.HelpBox("seedMin must be less than seedMax.", MessageType.Error);
        }
    }

    void DrawGravitySection()
    {
        gravityFoldout = EditorGUILayout.Foldout(gravityFoldout, "Gravity Area", true);
        if (!gravityFoldout) return;

        EditorGUILayout.PropertyField(addGravityArea);

        if (!addGravityArea.boolValue) return;

        EditorGUILayout.PropertyField(gravityAreaMultiplier);
        EditorGUILayout.PropertyField(gravityAreaIsTrigger);
        EditorGUILayout.PropertyField(gravityComponentTypeName);

        if (string.IsNullOrEmpty(gravityComponentTypeName.stringValue))
            EditorGUILayout.HelpBox("Enter the name of the gravity component (eg. GravityAreaPoint).", MessageType.Warning);
    }

    void DrawTagLayerSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag & Layer", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(planetTag);
        EditorGUILayout.PropertyField(planetLayerName);

        if (!IsTagDefined(planetTag.stringValue))
            EditorGUILayout.HelpBox($"Tag '{planetTag.stringValue}' doesn't exist", MessageType.Warning);

        if (LayerMask.NameToLayer(planetLayerName.stringValue) < 0)
            EditorGUILayout.HelpBox($"Layer '{planetLayerName.stringValue}' doesn't exist.", MessageType.Warning);
    }

    void DrawActionsSection()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Spawn Planets"))
        {
            ((PlanetSpawnerManager)target).SpawnAll();
        }

        if (GUILayout.Button("Clear Children"))
        {
            ClearChildren();
        }

        EditorGUILayout.EndHorizontal();
    }

    void ClearChildren()
    {
        var t = ((PlanetSpawnerManager)target).transform;
        while (t.childCount > 0)
        {
            DestroyImmediate(t.GetChild(0).gameObject);
        }
    }

    bool IsTagDefined(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        try
        {
            GameObject.FindWithTag(tag);
            return true;
        }
        catch { return false; }
    }
}
