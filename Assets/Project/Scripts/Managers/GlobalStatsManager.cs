using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalStatsManager : Singleton<GlobalStatsManager>
{
    [Header("Settings")]
    public bool dmgNumbers = false;
    public GameObject damageNumberPrefab;
    public bool healthBars = false;

    public float attackSpeed = 1;

    [Header("Global Stats")]
    public float protectiveEfficiency = 1;
    public float dexterityEfficiency = 1;
    public float offensiveEfficiency = 1;
    public float physicalEfficiency = 1;
    public float overallEffectiveness = 1;

    public float fireDamage = 1;
    public float iceDamage = 1;

    [Range(0,1)] public float enemyResist = 0;

    public float enemyDamage = 4;
    [Header("PreojectileStats")]
    public int additionalProjectiles = 0;
    public int additionalBounces = 0;




    public StatModifierCard statCardPrefab;
    public Card abilityPrefab;

    public List<SpellSO> spells = new List<SpellSO>();
    public List<Stat> allStats = new List<Stat>();

    public Transform rollContent;

    List<GameObject> instances = new List<GameObject>();

    void Start()
    {
        GeneratePicks();   
    }
public void GeneratePicks()
{
    instances.ForEach(i => Destroy(i));
    instances.Clear();
    
    List<SpellSO> tempSpells = new List<SpellSO>(spells);
    List<Stat> tempStats = new List<Stat>(allStats);

    for(int i = 0; i < 3; i++)
    {
        // Determine what type to generate based on availability
        bool generateStat = false;
        
        if (tempStats.Count > 0 && tempSpells.Count > 0)
        {
            // Both are available, randomly choose
            generateStat = Random.Range(0, 2) == 0;
        }
        else if (tempStats.Count > 0)
        {
            // Only stats are available
            generateStat = true;
        }
        else if (tempSpells.Count > 0)
        {
            // Only spells are available
            generateStat = false;
        }
        else
        {
            // Nothing left to generate
            Debug.LogWarning("Ran out of both stats and spells to generate.");
            break;
        }
        
        // Generate the appropriate card
        if (generateStat)
        {
            StatModifierCard card = Instantiate(statCardPrefab, rollContent);
            Stat randomStat = GetRandomStat(tempStats);
            tempStats.Remove(randomStat);
            card.Setup(randomStat);
            instances.Add(card.gameObject);
        }
        else
        {
            Card card = Instantiate(abilityPrefab, rollContent);
            SpellSO randomSpell = tempSpells[Random.Range(0, tempSpells.Count)];
            tempSpells.Remove(randomSpell);
            card.Setup(randomSpell);
            instances.Add(card.gameObject);
        }
    }
}
// Keep your GetRandomStat method as is
public Stat GetRandomStat(List<Stat> statsList)
{
    float totalWeight = statsList.Sum(stat => stat.chanceOfDropping);
    if (totalWeight <= 0f)
    {
        Debug.LogWarning("No stats have a drop chance greater than zero.");
        return null;
    }
    
    float randomPoint = Random.value * totalWeight;
    float current = 0f;
    foreach (var stat in statsList)
    {
        current += stat.chanceOfDropping;
        if (randomPoint <= current)
        {
            // Create a copy of the stat
            Stat chosenStat = new Stat
            {
                type = stat.type,
                minValue = stat.minValue,
                maxValue = stat.maxValue,
                format = stat.format,
                smallDescription = stat.smallDescription,
                chanceOfDropping = stat.chanceOfDropping
            };
            chosenStat.RollValue();
            
            return stat; // Return the original for removal
        }
    }
    return null; // fallback
}
}
