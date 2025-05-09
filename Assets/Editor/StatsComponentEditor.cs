using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(StatModifier))]
public class StatModifierEditor : Editor
{
    private string[] statFieldOptions;

    void OnEnable()
    {
        // Generate the field options based on the target class fields
        var fields = typeof(StatsComponent).GetFields();
        List<string> valid = new();
        foreach (var f in fields)
        {
            if (f.FieldType == typeof(int) || f.FieldType == typeof(float))
                valid.Add(f.Name);
        }
        statFieldOptions = valid.ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var modifier = (StatModifier)target;

        SerializedProperty statPool = serializedObject.FindProperty("statPool");
        EditorGUILayout.LabelField("Stat Pool", EditorStyles.boldLabel);

        for (int i = 0; i < statPool.arraySize; i++)
        {
            var entry = statPool.GetArrayElementAtIndex(i);
            var targetField = entry.FindPropertyRelative("targetField");

            int currentIndex = Array.IndexOf(statFieldOptions, targetField.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            // Dropdown to select stat type (targetField)
            int newIndex = EditorGUILayout.Popup("Stat", currentIndex, statFieldOptions);
            targetField.stringValue = statFieldOptions[newIndex];

            // Fields for Min and Max values
            EditorGUILayout.PropertyField(entry.FindPropertyRelative("minValue"));
            EditorGUILayout.PropertyField(entry.FindPropertyRelative("maxValue"));

            // Remove button for the stat option
            if (GUILayout.Button($"Remove Stat Option {i}"))
            {
                statPool.DeleteArrayElementAtIndex(i);
                break; // Break the loop as array size has changed
            }

            EditorGUILayout.Space();
        }

        // Button to add a new stat option
        if (GUILayout.Button("Add Stat Option"))
        {
            statPool.arraySize++;
        }

        // Property fields for other settings
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfStatsToGenerate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("generateSingleLowPositive"));

        // Generate stats button
        if (GUILayout.Button("Generate Stats"))
        {
            modifier.Generate();
            EditorUtility.SetDirty(modifier);
        }

        // Apply and remove stats buttons
        if (GUILayout.Button("Apply Stats"))
        {
            StatsComponent targetComponent = FindObjectOfType<StatsComponent>();
            modifier.ApplyStats(targetComponent);
            EditorUtility.SetDirty(modifier);
        }

        if (GUILayout.Button("Remove Stats"))
        {
            StatsComponent targetComponent = FindObjectOfType<StatsComponent>();
            modifier.RemoveStats(targetComponent);
            EditorUtility.SetDirty(modifier);
        }

        serializedObject.ApplyModifiedProperties();
    }
}