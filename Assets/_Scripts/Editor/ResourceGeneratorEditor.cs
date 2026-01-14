using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceGenerator))]
public class ResourceGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(8);

        if (GUILayout.Button("Save current config as SO"))
            CreateConfigFromCurrent();
    }

    void CreateConfigFromCurrent()
    {
        var gen = (ResourceGenerator)target;
        var path = EditorUtility.SaveFilePanelInProject("Create Resource Generator Config", "ResourceGeneratorConfig", "asset", "");
        if (string.IsNullOrEmpty(path)) return;

        var asset = ScriptableObject.CreateInstance<ResourceGeneratorConfig>();

        var so = new SerializedObject(gen);
        var resProp = so.FindProperty("resources");
        var propsProp = so.FindProperty("props");

        asset.resources = CopyItems(resProp);
        asset.props = CopyItems(propsProp);

        asset.collisionCheckRadius = so.FindProperty("collisionCheckRadius").floatValue;
        asset.rotationAdjustment = so.FindProperty("rotationAdjustment").vector3Value;
        asset.waterMargin = so.FindProperty("waterMargin").floatValue;
        asset.oceanResolveTimeout = so.FindProperty("oceanResolveTimeout").floatValue;
        asset.maxAttemptsPerInstance = so.FindProperty("maxAttemptsPerInstance").intValue;

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var genSO = new SerializedObject(gen);
        genSO.FindProperty("config").objectReferenceValue = asset;
        genSO.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        EditorUtility.SetDirty(gen);
        Debug.Log($"Config has been created and assigned: {path}");
    }

    ResourceGenerator.SpawnItem[] CopyItems(SerializedProperty arrayProp)
    {
        if (arrayProp == null || !arrayProp.isArray) return new ResourceGenerator.SpawnItem[0];

        int n = arrayProp.arraySize;
        var dst = new ResourceGenerator.SpawnItem[n];

        for (int i = 0; i < n; i++)
        {
            var el = arrayProp.GetArrayElementAtIndex(i);
            dst[i] = new ResourceGenerator.SpawnItem
            {
                prefab = (GameObject)el.FindPropertyRelative("prefab").objectReferenceValue,
                count = el.FindPropertyRelative("count").intValue,
                alignAxis = (ResourceGenerator.AlignAxis)el.FindPropertyRelative("alignAxis").enumValueIndex,
                offset = el.FindPropertyRelative("offset").floatValue,
                avoidWater = el.FindPropertyRelative("avoidWater").boolValue,
                initialized = true
            };
        }

        return dst;
    }
}