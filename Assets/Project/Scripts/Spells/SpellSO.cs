using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/NewSpell")]
public class SpellSO : ScriptableObject
{
    public List<SpellAttributes> attributes = new List<SpellAttributes>();

    public SpellCategory spellCategory;

    public AbilityType abilityType;
    public string spellName;
    public string spellDescription;
    public Sprite spellIcon;
    public float cooldownDuration = 5f;  // Example cooldown time in seconds

    [Header("Object")]
    public GameObject spellPrefab; // Prefab of the projectile
    public Vector3 spawnOffset;

    [Header("Projectile")]

    public int multicastCount = 1;
    public int multicastMaxAngle = 90;

    [Header("Duration")]
    public float spellDuration = 0f;
    public bool canBeCanceled = false;

    [Header("Chain")]
    public int bounces = 0;

    public bool isGrounded;


    public float baseValue = 1;
    public float multiplierPerLevel = 0.4f;
    public int abilityLevel = 1;

    Card myCard;
    GlobalStatsManager gsm;
    public void Initialize(Card myCardInstance)
    {
        myCard = myCardInstance;
        gsm = GlobalStatsManager.Instance;
        UpdateCard();
    }

    public void UpdateCard()
    {
        myCard.Setup(this);
    }
    public float ProccessedValue()
    {
        float efficiency = 1;
        switch(spellCategory)
        {
            case SpellCategory.Power:
            efficiency = gsm.offensiveEfficiency;
            break;
            case SpellCategory.Protective:
            efficiency = gsm.protectiveEfficiency;
            break;
            case SpellCategory.Dexterity:
            efficiency = gsm.dexterityEfficiency;
            break;
        }
        return ((multiplierPerLevel*abilityLevel)*baseValue) * efficiency * gsm.overallEffectiveness ;
    }
    public int GetBounces()
    {
        return bounces + gsm.additionalBounces;
    }

    public int GetCasts()
    {
        return multicastCount + gsm.additionalBounces;
    }

    public void LevelUp(Card card)
    {
        if(card.SpellSO == this)
        {
            Destroy(card.gameObject);
            abilityLevel++;
            UpdateCard();
        }
    }
}
public enum SpellAttributes
{
    Protective,
    Fire,
    Ice,
    Multicast,
    AOE,
    Chain,
    Duration,
}

public enum SpellCategory
{
    Power,
    Dexterity,
    Protective,
}

public enum AbilityType
{
    Cast,
    AfflictUser
}

