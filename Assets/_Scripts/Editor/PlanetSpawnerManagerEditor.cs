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

    private PlanetSpawnerManager _component;

    void OnEnable()
    {
        _component = (PlanetSpawnerManager)target;

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

        GenerateMainHeaderLabel("Planet Spawner Manager");
        DrawPresetsSection();

        //GenerateColoredHeader("Spawning Configuration", new Color(1f, 1f, 0.8f));
        DrawSpawnSection();

        //GenerateColoredHeader("Gravity Area", new Color(0.9f, 0.9f, 1f));
        DrawGravitySection();

        //GenerateColoredHeader("Tag & Layer", new Color(1f, 0.8f, 0.9f));
        DrawTagLayerSection();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawPresetsSection()
    {
        presets.isExpanded = GenerateFoldout(presets.isExpanded, "Presets", new Color(1f, 0.996f, 0.702f, 1f));
        if (!presets.isExpanded) return;

        EditorGUILayout.Space(10);
        EditorGUI.indentLevel++;

        for (int i = 0; i < presets.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i}", GUILayout.Width(30));
            SerializedProperty element = presets.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(element, GUIContent.none, true);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(30)))
            presets.arraySize++;

        if (GUILayout.Button("-", GUILayout.Width(30)) && presets.arraySize > 0)
            presets.arraySize--;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUI.indentLevel--;

        if (presets.arraySize == 0)
            EditorGUILayout.HelpBox("Add at least 1 CelestialBodySettings preset.", MessageType.Warning);
    }

    void DrawSpawnSection()
    {
        spawnFoldout = GenerateFoldout(spawnFoldout, "Spawning Configuration", new Color(0.898f, 1f, 0.89f));
        if (!spawnFoldout) return;

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(130));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Spawn Count", GUILayout.Width(180));
        EditorGUILayout.PropertyField(spawnCount, GUIContent.none, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Spawn Radius", GUILayout.Width(180));
        EditorGUILayout.PropertyField(spawnRadius, GUIContent.none, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scale Range", GUILayout.Width(140));
        EditorGUILayout.PropertyField(scaleRange, GUIContent.none, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        GUIStyle blueLabelStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.651f, 0.953f, 0.961f) } };

        EditorGUILayout.LabelField("Randomize Seed", blueLabelStyle, GUILayout.Width(180));
        EditorGUILayout.PropertyField(randomizeSeed, GUIContent.none, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        if (randomizeSeed.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Seed Min", GUILayout.Width(180));
            EditorGUILayout.PropertyField(seedMin, GUIContent.none, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Seed Max", GUILayout.Width(180));
            EditorGUILayout.PropertyField(seedMax, GUIContent.none, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            if (seedMin.intValue >= seedMax.intValue)
                EditorGUILayout.HelpBox("Seed Min must be less than Seed Max.", MessageType.Error);
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Parent Under This", GUILayout.Width(180));
        EditorGUILayout.PropertyField(parentUnderThis, GUIContent.none, GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);
    }

    void DrawGravitySection()
    {
        gravityFoldout = GenerateFoldout(gravityFoldout, "Gravity Area", new Color(0.863f, 0.753f, 1f));
        if (!gravityFoldout) return;

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(130));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Gravity is Trigger", GUILayout.Width(180));
        EditorGUILayout.PropertyField(gravityAreaIsTrigger, GUIContent.none, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Add Gravity Area", GUILayout.Width(180));
        EditorGUILayout.PropertyField(addGravityArea, GUIContent.none, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        if (addGravityArea.boolValue)
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gravity Multiplier", GUILayout.Width(180));
            EditorGUILayout.PropertyField(gravityAreaMultiplier, GUIContent.none, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component Type Name", GUILayout.Width(180));
            EditorGUILayout.PropertyField(gravityComponentTypeName, GUIContent.none, GUILayout.Width(140));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    void DrawTagLayerSection()
    {
        EditorGUILayout.Space(5);
        GenerateColoredHeader("Tag & Layer", new Color(1f, 0.831f, 0.875f));

        GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        GUIStyle centeredTextFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Planet Tag", centeredLabelStyle, GUILayout.Width(150));
        planetTag.stringValue = EditorGUI.TextField(EditorGUILayout.GetControlRect(GUILayout.Width(150)), planetTag.stringValue, centeredTextFieldStyle);
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Planet Layer", centeredLabelStyle, GUILayout.Width(150));
        planetLayerName.stringValue = EditorGUI.TextField(EditorGUILayout.GetControlRect(GUILayout.Width(150)), planetLayerName.stringValue, centeredTextFieldStyle);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (!IsTagDefined(planetTag.stringValue))
            EditorGUILayout.HelpBox($"Tag '{planetTag.stringValue}' doesn't exist.", MessageType.Warning);

        if (LayerMask.NameToLayer(planetLayerName.stringValue) < 0)
            EditorGUILayout.HelpBox($"Layer '{planetLayerName.stringValue}' doesn't exist.", MessageType.Warning);

        GUILayout.Space(20);
    }

    bool IsTagDefined(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        try
        {
            GameObject.FindWithTag(tag);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void GenerateMainHeaderLabel(string text)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fixedHeight = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle layoutStyle = new GUIStyle()
        {
            fixedHeight = 30,
            margin = { bottom = 10 }
        };

        EditorGUILayout.BeginHorizontal(layoutStyle);
        EditorGUILayout.LabelField(text, labelStyle);
        EditorGUILayout.EndHorizontal();
    }


    private void GenerateColoredHeader(string text, Color color)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            fixedHeight = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color }
        };

        GUIStyle layoutStyle = new GUIStyle()
        {
            fixedHeight = 30,
            margin = { bottom = 5 }
        };

        EditorGUILayout.BeginHorizontal(layoutStyle);
        EditorGUILayout.LabelField(text, labelStyle);
        EditorGUILayout.EndHorizontal();
    }

    private bool GenerateFoldout(bool foldoutState, string text, Color color)
    {
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = color },
            onNormal = { textColor = color }
        };

        Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), foldoutStyle, GUILayout.ExpandWidth(false));
        return EditorGUI.Foldout(rect, foldoutState, text, true, foldoutStyle);
    }
}