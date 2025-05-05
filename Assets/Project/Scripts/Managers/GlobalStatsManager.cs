using UnityEngine;

public class GlobalStatsManager : Singleton<GlobalStatsManager>
{
    [Header("Settings")]
    public bool dmgNumbers = false;
    public GameObject damageNumberPrefab;
    public bool healthBars = false;

    [Header("Global Stats")]
    public float protectiveEfficiency = 1;
    public float supportEfficiency = 1;
    public float offensiveEfficiency = 1;
    public float physicalEfficiency = 1;
    public int overallEffectiveness = 1;
    [Header("PreojectileStats")]
    public int additionalProjectiles = 0;
    public int additionalBounces = 0;
}
