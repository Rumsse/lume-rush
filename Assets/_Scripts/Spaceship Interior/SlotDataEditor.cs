using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SlotData))]
public class SlotDataEditor : Editor
{
    SerializedProperty stationName;
    SerializedProperty stationLevel;
    SerializedProperty stationBaseSpeed;
    SerializedProperty stationBaseSlotAmount;
    SerializedProperty stationBaseSlotCapacity;
    SerializedProperty stationBaseMaterialSaveChance;
    SerializedProperty stationBaseCapacity;
    private void OnEnable()
    {
        stationName = serializedObject.FindProperty("stationName");
        stationLevel = serializedObject.FindProperty("stationLevel");
        stationBaseSpeed = serializedObject.FindProperty("stationBaseSpeed");
        stationBaseSlotAmount = serializedObject.FindProperty("stationBaseSlotAmount");
        stationBaseSlotCapacity = serializedObject.FindProperty("stationBaseSlotCapacity");
        stationBaseMaterialSaveChance = serializedObject.FindProperty("stationBaseMaterialSaveChance");
        stationBaseCapacity = serializedObject.FindProperty("stationBaseCapacity");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SlotData sd = (SlotData)target;

        EditorGUILayout.PropertyField(stationName);
        EditorGUILayout.PropertyField(stationLevel);

        EditorGUILayout.Space();

        switch (stationName.stringValue.ToLower())
        {
            case "workshop":
                EditorGUILayout.PropertyField(stationBaseSpeed);
                EditorGUILayout.PropertyField(stationBaseSlotAmount);
                EditorGUILayout.PropertyField(stationBaseSlotCapacity);
                EditorGUILayout.PropertyField(stationBaseMaterialSaveChance);
                break;

            case "kitchen":
                EditorGUILayout.PropertyField(stationBaseSpeed);
                EditorGUILayout.PropertyField(stationBaseSlotAmount);
                EditorGUILayout.PropertyField(stationBaseSlotCapacity);
                break;

            case "cabin":
                EditorGUILayout.PropertyField(stationBaseCapacity);
                break;

            case "storage":
                EditorGUILayout.PropertyField(stationBaseSlotAmount);
                EditorGUILayout.PropertyField(stationBaseSlotCapacity);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
