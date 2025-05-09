using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Stats/Stat Modifier")]
public class StatModifier : ScriptableObject
{
    [System.Serializable]
    public class StatOption
    {
        public string targetField;
        public float minValue;
        public float maxValue;
    }

    [System.Serializable]
    public class GeneratedStat
    {
        public string targetField;
        public string valueToAdd;
    }

    public List<StatOption> statPool = new();
    public List<GeneratedStat> currentGenerated = new();

    [Range(1, 10)] public int numberOfStatsToGenerate = 1;
    public bool generateSingleLowPositive = false;  // Generate one positive stat with a low value

    public void Generate()
    {
        currentGenerated.Clear();
        List<StatOption> available = new(statPool);

        if (generateSingleLowPositive)
        {
            if (available.Count == 0) return;
            int index = UnityEngine.Random.Range(0, available.Count);
            StatOption selected = available[index];

            // Generate a small positive stat
            float lowPositive = UnityEngine.Random.Range(selected.minValue, selected.minValue + (selected.maxValue - selected.minValue) * 0.25f);
            lowPositive = Mathf.Max(0.1f, lowPositive); // Ensure it's positive and non-zero

            float adjusted = AdjustToFieldType(selected.targetField, lowPositive);
            currentGenerated.Add(new GeneratedStat
            {
                targetField = selected.targetField,
                valueToAdd = adjusted.ToString()
            });
            return;
        }

        // Normal stat generation
        for (int i = 0; i < numberOfStatsToGenerate && available.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, available.Count);
            StatOption selected = available[index];
            available.RemoveAt(index);

            if (currentGenerated.Exists(s => s.targetField == selected.targetField))
            {
                i--;
                continue;
            }

            // Randomly roll for stat value
            float baseRoll = UnityEngine.Random.Range(selected.minValue, selected.maxValue);
            float finalValue = baseRoll;

            finalValue = AdjustToFieldType(selected.targetField, finalValue);

            currentGenerated.Add(new GeneratedStat
            {
                targetField = selected.targetField,
                valueToAdd = finalValue.ToString()
            });
        }
    }

    private float AdjustToFieldType(string fieldName, float value)
    {
        var field = typeof(StatsComponent).GetField(fieldName);
        if (field != null && field.FieldType == typeof(int))
            return Mathf.Round(value);
        return value;
    }

    public void ApplyStats(StatsComponent target)
    {
        foreach (var stat in currentGenerated)
        {
            var field = typeof(StatsComponent).GetField(stat.targetField);
            if (field != null)
            {
                if (field.FieldType == typeof(int))
                {
                    int current = (int)field.GetValue(target);
                    int add = int.Parse(stat.valueToAdd);
                    field.SetValue(target, current + add);
                }
                else if (field.FieldType == typeof(float))
                {
                    float current = (float)field.GetValue(target);
                    float add = float.Parse(stat.valueToAdd);
                    field.SetValue(target, current + add);
                }
            }
        }
    }

    public void RemoveStats(StatsComponent target)
    {
        foreach (var stat in currentGenerated)
        {
            var field = typeof(StatsComponent).GetField(stat.targetField);
            if (field != null)
            {
                if (field.FieldType == typeof(int))
                {
                    int current = (int)field.GetValue(target);
                    int add = int.Parse(stat.valueToAdd);
                    field.SetValue(target, current - add);
                }
                else if (field.FieldType == typeof(float))
                {
                    float current = (float)field.GetValue(target);
                    float add = float.Parse(stat.valueToAdd);
                    field.SetValue(target, current - add);
                }
            }
        }
        currentGenerated.Clear();
    }
}