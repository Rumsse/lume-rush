using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlanetSpawnerManager))]
public class PlanetSpawnerManagerEditor : Editor
{
    #region Serialized

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

    #endregion

    #region State

    ReorderableList presetsList;
    SearchField typeSearch;
    string typeFilter = string.Empty;

    bool spawnFoldout = true;
    bool gravityFoldout = true;
    bool showSceneHandles;
    bool autoInvokeInPlaymode;

    int selectedMethodIndex = -1;
    List<MethodInfo> invokableMethods = new();

    static GUIStyle headerBig;
    static GUIStyle headerSmall;
    static GUIStyle centeredLabel;
    static GUIStyle foldoutStyle;

    PlanetSpawnerManager comp;

    const string Key_ShowHandles = "PlanetSpawnerEditor_ShowHandles";
    const string Key_AutoInvoke = "PlanetSpawnerEditor_AutoInvoke";
    const float LabelWidth = 180f;

    const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    FieldInfo fiSpawnRadius;
    FieldInfo fiSpawnCount;

    #endregion

    #region Unity

    void OnEnable()
    {
        comp = (PlanetSpawnerManager)target;

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

        BuildPresetsList();
        RebuildInvokableMethods();

        typeSearch ??= new SearchField();

        showSceneHandles = SessionState.GetBool(Key_ShowHandles, true);
        autoInvokeInPlaymode = SessionState.GetBool(Key_AutoInvoke, false);

        fiSpawnRadius = comp.GetType().GetField("spawnRadius", FieldFlags);
        fiSpawnCount = comp.GetType().GetField("spawnCount", FieldFlags);

        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeChanged;

    public override void OnInspectorGUI()
    {
        EnsureStyles();
        serializedObject.Update();

        DrawToolbar();
        DrawHeader("Planet Spawner Manager");
        DrawPresets();
        DrawSpawn();
        DrawGravity();
        DrawTagLayer();

        var changed = serializedObject.ApplyModifiedProperties();
        if (Application.isPlaying && autoInvokeInPlaymode && changed) SafeInvokeSelectedMethod();
    }

    void OnSceneGUI()
    {
        if (!showSceneHandles) return;
        if (comp == null) return;

        var pos = comp.transform.position;
        var rot = Quaternion.identity;
        var r = GetFloat(fiSpawnRadius, comp, 0f);

        Handles.color = new Color(0.2f, 0.9f, 0.6f, 0.8f);
        EditorGUI.BeginChangeCheck();
        var newR = Handles.RadiusHandle(rot, pos, Mathf.Max(0.01f, r));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(comp, "Adjust Spawn Radius");
            SetFloat(fiSpawnRadius, comp, Mathf.Max(0f, newR));
            EditorUtility.SetDirty(comp);
        }

        var displayR = GetFloat(fiSpawnRadius, comp, 0f);
        Handles.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Handles.DrawWireDisc(pos, Vector3.up, displayR);
        Handles.DrawWireDisc(pos, Vector3.right, displayR);
        Handles.DrawWireDisc(pos, Vector3.forward, displayR);

        Handles.BeginGUI();
        var sceneView = SceneView.currentDrawingSceneView;
        if (sceneView != null)
        {
            var rect = new Rect(12, 12, 280, 58);
            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            GUILayout.Label($"Radius: {displayR:0.##}", EditorStyles.boldLabel);
            GUILayout.Label($"Count: {GetInt(fiSpawnCount, comp, 0)}", EditorStyles.miniLabel);
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    #endregion

    #region GUI

    void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Refresh Methods", EditorStyles.toolbarButton, GUILayout.Width(120))) RebuildInvokableMethods();

            using (new EditorGUI.DisabledScope(invokableMethods.Count == 0))
            {
                var names = invokableMethods.Select(m => m.Name).ToArray();
                selectedMethodIndex = Mathf.Clamp(selectedMethodIndex, invokableMethods.Count > 0 ? 0 : -1, invokableMethods.Count - 1);
                var label = selectedMethodIndex >= 0 ? names[selectedMethodIndex] : "No methods";
                if (EditorGUILayout.DropdownButton(new GUIContent(label), FocusType.Keyboard, EditorStyles.toolbarDropDown, GUILayout.Width(160)))
                {
                    var menu = new GenericMenu();
                    for (int i = 0; i < names.Length; i++)
                    {
                        var idx = i;
                        menu.AddItem(new GUIContent(names[i]), selectedMethodIndex == idx, () => selectedMethodIndex = idx);
                    }
                    menu.ShowAsContext();
                }

                if (GUILayout.Button("Invoke", EditorStyles.toolbarButton, GUILayout.Width(70))) SafeInvokeSelectedMethod();
            }

            GUILayout.FlexibleSpace();

            showSceneHandles = GUILayout.Toggle(showSceneHandles, "Scene Handles", EditorStyles.toolbarButton, GUILayout.Width(110));
            autoInvokeInPlaymode = GUILayout.Toggle(autoInvokeInPlaymode, "Auto Invoke (Play)", EditorStyles.toolbarButton, GUILayout.Width(140));

            SessionState.SetBool(Key_ShowHandles, showSceneHandles);
            SessionState.SetBool(Key_AutoInvoke, autoInvokeInPlaymode);
        }
    }

    void DrawHeader(string text)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(text, headerBig, GUILayout.Height(28));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.Space(4);
    }

    void DrawPresets()
    {
        presets.isExpanded = DrawFoldout(presets.isExpanded, "Presets", new Color(1f, 0.996f, 0.702f));
        if (!presets.isExpanded) return;

        EditorGUILayout.Space(6);
        presetsList.DoLayoutList();

        if (presets.arraySize == 0)
            EditorGUILayout.HelpBox("Add at least 1 CelestialBodySettings preset.", MessageType.Warning);
    }

    void DrawSpawn()
    {
        spawnFoldout = DrawFoldout(spawnFoldout, "Spawning Configuration", new Color(0.898f, 1f, 0.89f));
        if (!spawnFoldout) return;

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Spawn Count", GUILayout.Width(LabelWidth));
                spawnCount.intValue = EditorGUILayout.IntField(spawnCount.intValue, GUILayout.Width(60));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Spawn Radius", GUILayout.Width(LabelWidth));
                spawnRadius.floatValue = EditorGUILayout.FloatField(spawnRadius.floatValue, GUILayout.Width(60));
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Scale Range", GUILayout.Width(LabelWidth));
                var v = scaleRange.vector2Value;
                var min = v.x;
                var max = v.y;
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0.0001f, Mathf.Max(0.0001f, max <= 0 ? 10f : Math.Max(max, 10f)));
                min = Mathf.Max(0.0001f, min);
                max = Mathf.Max(min, max);
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(160)))
                {
                    min = EditorGUILayout.FloatField(min, GUILayout.Width(70));
                    max = EditorGUILayout.FloatField(max, GUILayout.Width(70));
                }
                scaleRange.vector2Value = new Vector2(min, max);
            }

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Randomize Seed", headerSmall, GUILayout.Width(LabelWidth));
                EditorGUILayout.PropertyField(randomizeSeed, GUIContent.none, GUILayout.Width(70));
            }

            if (randomizeSeed.boolValue)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Seed Min", GUILayout.Width(LabelWidth));
                    seedMin.intValue = EditorGUILayout.IntField(seedMin.intValue, GUILayout.Width(80));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Seed Max", GUILayout.Width(LabelWidth));
                    seedMax.intValue = EditorGUILayout.IntField(seedMax.intValue, GUILayout.Width(80));
                }

                if (seedMin.intValue >= seedMax.intValue)
                    EditorGUILayout.HelpBox("Seed Min must be less than Seed Max.", MessageType.Error);
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Parent Under This", GUILayout.Width(LabelWidth));
                EditorGUILayout.PropertyField(parentUnderThis, GUIContent.none);
            }
        }

        EditorGUILayout.Space(8);
    }

    void DrawGravity()
    {
        gravityFoldout = DrawFoldout(gravityFoldout, "Gravity Area", new Color(0.863f, 0.753f, 1f));
        if (!gravityFoldout) return;

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Gravity is Trigger", GUILayout.Width(LabelWidth));
                EditorGUILayout.PropertyField(gravityAreaIsTrigger, GUIContent.none, GUILayout.Width(50));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Add Gravity Area", GUILayout.Width(LabelWidth));
                EditorGUILayout.PropertyField(addGravityArea, GUIContent.none, GUILayout.Width(50));
            }

            if (addGravityArea.boolValue)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Gravity Multiplier", GUILayout.Width(LabelWidth));
                    gravityAreaMultiplier.floatValue = EditorGUILayout.FloatField(gravityAreaMultiplier.floatValue, GUILayout.Width(80));
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Component Type", GUILayout.Width(LabelWidth));
                        var shown = string.IsNullOrEmpty(gravityComponentTypeName.stringValue) ? "<select>" : gravityComponentTypeName.stringValue;
                        if (GUILayout.Button(shown, GUILayout.Height(18)))
                            ShowTypeMenu();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Filter", GUILayout.Width(60));
                        typeFilter = typeSearch.OnToolbarGUI(typeFilter);
                    }
                }
            }
        }
    }

    void DrawTagLayer()
    {
        EditorGUILayout.Space(4);
        DrawColoredHeader("Tag & Layer", new Color(1f, 0.831f, 0.875f));

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Planet Tag", centeredLabel, GUILayout.Width(150));
                planetTag.stringValue = EditorGUILayout.TagField(GUIContent.none, string.IsNullOrEmpty(planetTag.stringValue) ? "Untagged" : planetTag.stringValue, GUILayout.Width(150));
            }

            GUILayout.Space(16);

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("Planet Layer", centeredLabel, GUILayout.Width(150));
                var layerIndex = Mathf.Max(0, LayerMask.NameToLayer(string.IsNullOrEmpty(planetLayerName.stringValue) ? "Default" : planetLayerName.stringValue));
                var newLayer = EditorGUILayout.LayerField(GUIContent.none, layerIndex, GUILayout.Width(150));
                planetLayerName.stringValue = LayerMask.LayerToName(newLayer);
            }

            GUILayout.FlexibleSpace();
        }

        if (!IsTagDefined(planetTag.stringValue))
            EditorGUILayout.HelpBox($"Tag '{planetTag.stringValue}' doesn't exist.", MessageType.Warning);

        if (LayerMask.NameToLayer(planetLayerName.stringValue) < 0)
            EditorGUILayout.HelpBox($"Layer '{planetLayerName.stringValue}' doesn't exist.", MessageType.Warning);

        EditorGUILayout.Space(10);
    }

    #endregion

    #region Scene/Runtime Helpers

    void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        if (!autoInvokeInPlaymode) return;
        SafeInvokeSelectedMethod();
    }

    void SafeInvokeSelectedMethod()
    {
        if (selectedMethodIndex < 0 || selectedMethodIndex >= invokableMethods.Count) return;
        var m = invokableMethods[selectedMethodIndex];
        if (m == null) return;

        foreach (var t in targets)
        {
            try
            {
                if (!Application.isPlaying) Undo.RecordObject(t, $"Invoke {m.Name}");
                m.Invoke(t, null);
                EditorUtility.SetDirty((UnityEngine.Object)t);
            }
            catch { }
        }
    }

    void RebuildInvokableMethods()
    {
        invokableMethods.Clear();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var methods = comp.GetType().GetMethods(flags)
            .Where(mi => mi.GetParameters().Length == 0 && mi.ReturnType == typeof(void) && !mi.IsSpecialName)
            .OrderBy(mi => mi.IsPublic ? 0 : 1)
            .ThenBy(mi => mi.Name);

        invokableMethods.AddRange(methods);
        selectedMethodIndex = invokableMethods.Count > 0 ? 0 : -1;
    }

    #endregion

    #region Internal UI

    void BuildPresetsList()
    {
        presetsList = new ReorderableList(serializedObject, presets, true, true, true, true);

        presetsList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Presets");
        };

        presetsList.elementHeightCallback = index =>
        {
            var element = presets.GetArrayElementAtIndex(index);
            return Mathf.Max(EditorGUI.GetPropertyHeight(element, true), EditorGUIUtility.singleLineHeight) + 6f;
        };

        presetsList.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2;
            rect.height -= 4;
            using (new EditorGUI.IndentLevelScope(0))
            {
                var labelRect = new Rect(rect.x, rect.y, 26, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, index.ToString());
                var fieldRect = new Rect(rect.x + 28, rect.y, rect.width - 28, rect.height);
                EditorGUI.PropertyField(fieldRect, presets.GetArrayElementAtIndex(index), GUIContent.none, true);
            }
        };
    }

    void ShowTypeMenu()
    {
        var all = TypeCache.GetTypesDerivedFrom<Component>();
        IEnumerable<Type> filtered = all.Where(t => !t.IsAbstract && !t.IsGenericType && t.FullName != null);
        if (!string.IsNullOrWhiteSpace(typeFilter))
            filtered = filtered.Where(t => t.FullName.IndexOf(typeFilter, StringComparison.OrdinalIgnoreCase) >= 0);

        var list = filtered.OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToList();
        var menu = new GenericMenu();

        if (list.Count == 0) menu.AddDisabledItem(new GUIContent("No matches"));
        else
        {
            foreach (var t in list)
            {
                var name = string.IsNullOrEmpty(t.Namespace) ? t.Name : $"{t.Namespace}/{t.Name}";
                menu.AddItem(new GUIContent(name), gravityComponentTypeName.stringValue == t.FullName, () =>
                {
                    gravityComponentTypeName.stringValue = t.FullName;
                    serializedObject.ApplyModifiedProperties();
                });
            }
        }

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Clear"), string.IsNullOrEmpty(gravityComponentTypeName.stringValue), () =>
        {
            gravityComponentTypeName.stringValue = string.Empty;
            serializedObject.ApplyModifiedProperties();
        });

        menu.ShowAsContext();
    }

    bool DrawFoldout(bool state, string text, Color color)
    {
        foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        var prev = GUI.color;
        GUI.color = color;
        var r = GUILayoutUtility.GetRect(new GUIContent(text), foldoutStyle, GUILayout.ExpandWidth(true));
        state = EditorGUI.Foldout(r, state, text, true, foldoutStyle);
        GUI.color = prev;
        return state;
    }

    void DrawColoredHeader(string text, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(text, headerSmall, GUILayout.Height(22));
            GUILayout.FlexibleSpace();
        }
        GUI.color = prev;
    }

    void EnsureStyles()
    {
        if (headerBig != null) return;

        headerBig = new GUIStyle(EditorStyles.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerBig.normal.textColor = Color.white;

        headerSmall = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };

        centeredLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
    }

    #endregion

    #region Utils

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

    static float GetFloat(FieldInfo fi, object obj, float fallback) => fi != null && obj != null ? (float)fi.GetValue(obj) : fallback;
    static void SetFloat(FieldInfo fi, object obj, float value) { if (fi == null || obj == null) return; fi.SetValue(obj, value); }
    static int GetInt(FieldInfo fi, object obj, int fallback) => fi != null && obj != null ? (int)fi.GetValue(obj) : fallback;

    #endregion
}