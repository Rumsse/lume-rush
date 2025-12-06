using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindAndFixPinkMaterials : EditorWindow
{
    string fallbackShaderName = "Standard";
    Vector2 scroll;
    string[] foundPaths;

    [MenuItem("Tools/Find Pink Materials")]
    static void Open() => GetWindow<FindAndFixPinkMaterials>("Pink Materials Finder");

    void OnGUI()
    {
        GUILayout.Label("ZnajdŸ materia³y z brakuj¹cymi/ b³êdnymi shaderami", EditorStyles.boldLabel);

        fallbackShaderName = EditorGUILayout.TextField("Shader zastêpczy (nazwa)", fallbackShaderName);

        if (GUILayout.Button("Skanuj projekt"))
            Scan();

        if (foundPaths != null && foundPaths.Length > 0)
        {
            GUILayout.Space(6);
            GUILayout.Label($"Znaleziono {foundPaths.Length} materia³ów:");
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(200));
            foreach (var p in foundPaths)
                GUILayout.Label(p);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zaznacz w Project Window"))
                SelectInProject();
            if (GUILayout.Button("Pinguj pierwszy"))
                PingFirst();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Zamieñ shader dla wszystkich na fallback"))
                ReplaceAllWithFallback();
        }
        else
        {
            GUILayout.Space(6);
            GUILayout.Label("Brak wyników (uruchom skan)", EditorStyles.miniLabel);
        }
    }

    void Scan()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        var list = new List<string>();
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            var sh = mat.shader;
            if (sh == null || sh.name == "Hidden/InternalErrorShader" || sh.name.ToLower().Contains("error"))
                list.Add(path);
        }
        foundPaths = list.ToArray();
        if (foundPaths.Length == 0)
            EditorUtility.DisplayDialog("Pink Materials", "Nie znaleziono materia³ów z b³êdnymi shaderami", "OK");
    }

    void SelectInProject()
    {
        var objs = new List<UnityEngine.Object>();
        foreach (var p in foundPaths)
        {
            var o = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (o != null) objs.Add(o);
        }
        Selection.objects = objs.ToArray();
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    void PingFirst()
    {
        if (foundPaths.Length == 0) return;
        var o = AssetDatabase.LoadAssetAtPath<Material>(foundPaths[0]);
        if (o != null) EditorGUIUtility.PingObject(o);
    }

    void ReplaceAllWithFallback()
    {
        var shader = Shader.Find(fallbackShaderName);
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Shader nie znaleziony", $"Shader '{fallbackShaderName}' nie istnieje w projekcie.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Replace pink material shaders");
        int group = Undo.GetCurrentGroup();
        int replaced = 0;
        foreach (var path in foundPaths)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            Undo.RecordObject(mat, "Replace shader");
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            replaced++;
        }
        AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(group);
        EditorUtility.DisplayDialog("Zrobione", $"Podmieniono shader na {replaced} materia³ach.", "OK");
    }
}