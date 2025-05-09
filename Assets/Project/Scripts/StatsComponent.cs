using System.Collections.Generic;
using UnityEngine;

public class StatsComponent : MonoBehaviour
{
    public int projectiles = 1;
    public float fireDamage = 5.0f;
    public float cooldownReduction = 0.1f;

    [Header("Applied Modifiers")]
    public List<StatModifier> appliedModifiers;

    public void ApplyAllModifiers()
    {
        foreach (var mod in appliedModifiers)
        {
            mod.ApplyStats(this);
        }
    }

    public void RemoveAllModifiers()
    {
        foreach (var mod in appliedModifiers)
        {
            mod.RemoveStats(this);
        }
    }
}