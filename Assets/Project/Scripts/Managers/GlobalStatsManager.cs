using UnityEngine;

public class GlobalStatsManager : Singleton<GlobalStatsManager>
{
    public float protectiveEfficiency = 1;
    public float supportEfficiency = 1;
    public float offensiveEfficiency = 1;
    public int overallEffectiveness = 1;
    [Header("PreojectileStats")]
    public int additionalProjectiles = 0;
    public int additionalBounces = 0;
}
