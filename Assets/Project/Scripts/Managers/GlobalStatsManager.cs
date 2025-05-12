using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalStatsManager : Singleton<GlobalStatsManager>
{
    public PlayerController player;
    [Header("Settings")]
    public bool dmgNumbers = false;
    public GameObject damageNumberPrefab;
    public bool healthBars = false;
    public float attackSpeed = 1;

    [Header("Global Stats")]
    public int floor=1;
    public float resistPerLevel = 0.02f;

    public float enemyHealthPerLevel = 10f;
    public float protectiveEfficiency = 0;
    public float dexterityEfficiency = 0;
    public float offensiveEfficiency = 0;
    public float physicalEfficiency = 0;
    public float overallEffectiveness = 0;

    public float fireDamage = 0;
    public float iceDamage = 0;

    public float cooldownReduction = 0;

    public float shopDiscount = 0;

    [Range(0,1)] public float enemyResist = 0;

    public float enemyDamage = 4;

    public float playerAdditionalHealth = 0;

    [Header("PreojectileStats")]
    public int additionalProjectiles = 0;
    public int additionalBounces = 0;




    public StatModifierCard statCardPrefab;
    public Card abilityPrefab;

    public List<SpellSO> spells = new List<SpellSO>();


    public List<Stat> allStats = new List<Stat>();

    public Transform rollContent;
    public GameObject rollsPannel;

    List<GameObject> instances = new List<GameObject>();

    public float GetResist()
    {
        return resistPerLevel * floor;
    }
    public float GetHealth()
    {

        return enemyHealthPerLevel * floor;
    }

    void Start()
    {
        spells.ForEach(s=>s.ResetToDefaults());
    }

    public bool IsPicksActive()
    {
        return rollsPannel.activeSelf;
    }
public void GeneratePicks()
{
    rollsPannel.SetActive(true);
    instances.ForEach(i => Destroy(i));
    instances.Clear();

    List<SpellSO> tempSpells = new List<SpellSO>(spells);
    List<Stat> tempStats = new List<Stat>(allStats);

    for (int i = 0; i < 3; i++)
    {
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
            if (tempStats.Count > 0) // Ensure there are still stats to generate
            {
                StatModifierCard card = Instantiate(statCardPrefab, rollContent);
                Stat randomStat = GetRandomStat(tempStats);
                randomStat.RollValue();
                tempStats.Remove(randomStat);
                card.Setup(randomStat);
                instances.Add(card.gameObject);
            }
        }
        else
        {
            if (tempSpells.Count > 0) // Ensure there are still spells to generate
            {
                Card card = Instantiate(abilityPrefab, rollContent);
                SpellSO randomSpell = tempSpells[Random.Range(0, tempSpells.Count)];
                tempSpells.Remove(randomSpell);
                card.Setup(randomSpell);
                instances.Add(card.gameObject);
            }
        }
    }
    instances.ForEach(i => i.AddComponent<Selectable>().Setup(()=>Pick(i)));
}
public List<SpellSO> GetRandomCards(int count)
{
    List<SpellSO> tempSpells = new List<SpellSO>(spells);

    List<SpellSO> pickedSpells = new List<SpellSO>();
    for(int i = 0; i < count; i++)
    {
        if(tempSpells.Count ==0)break;
        SpellSO randomSpell = tempSpells[Random.Range(0,tempSpells.Count)];
        tempSpells.Remove(randomSpell);
        pickedSpells.Add(randomSpell);
    }
    return pickedSpells;
}

public List<Stat> GetRandomStats(int count)
{
    List<Stat> tempSpells = new List<Stat>(allStats);

    List<Stat> pickedSpells = new List<Stat>();
    for(int i = 0; i < count; i++)
    {
        if(tempSpells.Count ==0)break;
        Stat randomSpell = tempSpells[Random.Range(0,tempSpells.Count)];
        randomSpell.RollValue();
        tempSpells.Remove(randomSpell);
        pickedSpells.Add(randomSpell);
    }
    return pickedSpells;
}
public void Pick(GameObject card)
{
    if(card.TryGetComponent(out Card c))
    {
        Instance.Pick(c);
    }
    else if(card.TryGetComponent(out StatModifierCard ca))
    {
        Pick(ca);
    }
}
public void Pick(Card card)
{
    rollContent.parent.gameObject.SetActive(false);

    CardManager.Instance.AddCard(card.spellSO);

}
public void Pick(StatModifierCard card)
{
    rollContent.parent.gameObject.SetActive(false);

    ApplyStat(card.myStat);
}
public void GenerateRandomCard()
{
    CardManager.Instance.AddCard(spells[Random.Range(0, spells.Count)]);
}
public void ApplyStat(Stat stat)
{
switch (stat.type)
{
    case StatType.Fire:
        fireDamage += stat.currentValue;
        break;
    case StatType.Ice:
        iceDamage += stat.currentValue;
        break;
    case StatType.Bounces:
        additionalBounces += (int)stat.currentValue;
        break;
    case StatType.Multicast:
        additionalProjectiles += (int)stat.currentValue;
        break;
    case StatType.Shop_Discount:
        shopDiscount+=stat.currentValue;
        shopDiscount = Mathf.Clamp(shopDiscount,0,0.50f);
        if(shopDiscount>=0.50f)
            allStats.Remove(stat);
        break;
    case StatType.Attack_Speed:
        attackSpeed+=stat.currentValue;
        attackSpeed = Mathf.Clamp(attackSpeed,0,2f);
        if(attackSpeed>=2f)
            allStats.Remove(stat);
        break;
    case StatType.Cooldown_Reduction:
        // Handle CooldownReduction
        cooldownReduction += cooldownReduction;
        cooldownReduction = Mathf.Clamp(cooldownReduction,0,0.70f);
        if(cooldownReduction>=0.70f)
            allStats.Remove(stat);
        break;
    case StatType.Power:
        offensiveEfficiency += stat.currentValue;
        break;
    case StatType.Dexterity:
        // Handle Dexterity
        break;
    case StatType.Protective:
        protectiveEfficiency += stat.currentValue;
        break;
    case StatType.Health:
        player.AddHealth(stat.currentValue);
        break;
    case StatType.Melee_Damage:
        physicalEfficiency += stat.currentValue;
        break;
}

CardManager.Instance.RefreshCard();
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
