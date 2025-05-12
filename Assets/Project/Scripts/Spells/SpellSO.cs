using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/NewSpell")]
public class SpellSO : ScriptableObject
{
    [Header("Core")]
    public List<SpellAttributes> attributes = new List<SpellAttributes>();
    public SpellCategory spellCategory;
    public AbilityType abilityType;

    public EffectType effectType;
    public string spellName;
    public string spellDescription;
    public Sprite spellIcon;
    public float cooldownDuration = 5f;

    [Header("Object")]
    public GameObject spellPrefab;
    public Vector3 spawnOffset;

    [Header("Projectile")]
    public int multicastCount = 1;
    public int multicastMaxAngle = 90;

    public bool useAngle;

    [Header("Duration")]
    public float spellDuration = 0f;
    public bool canBeCanceled = false;

    [Header("Chain")]
    public int bounces = 0;
    public bool isGrounded;

    public float effectorValue = 0.3f;

    public float baseValue = 1;
    public float multiplierPerLevel = 0.4f;
    public int abilityLevel = 1;

    // 🔒 Private default values for reset
    private int defaultMulticastCount;
    private int defaultBounces;
    private int defaultAbilityLevel;
    private float defaultSpellDuration;
    private float defaultCooldownDuration;
    private float defaultBaseValue;
    private float defaultMultiplierPerLevel;


    Card myCard;
    GlobalStatsManager gsm;

    public int shopPrice = 150;

    // Called when SO is loaded or recompiled
    private void OnEnable()
    {
        CacheDefaults();
    }

    public void CheckIfShouldApplyEffect(Damageable damageable)
    {
        damageable.ApplyEffect(effectType,effectorValue,spellDuration);
    }

    private void CacheDefaults()
    {
        defaultMulticastCount = multicastCount;
        defaultBounces = bounces;
        defaultAbilityLevel = abilityLevel;
        defaultSpellDuration = spellDuration;
        defaultCooldownDuration = cooldownDuration;
        defaultBaseValue = baseValue;
        defaultMultiplierPerLevel = multiplierPerLevel;
    }

    public void ResetToDefaults()
    {
        multicastCount = defaultMulticastCount;
        bounces = defaultBounces;
        abilityLevel = 1;
        spellDuration = defaultSpellDuration;
        cooldownDuration = defaultCooldownDuration;
        baseValue = defaultBaseValue;
        multiplierPerLevel = defaultMultiplierPerLevel;
        myCard = null;
        //UpdateCard();
    }

    public void Initialize(Card myCardInstance, bool changeMyCard)
    {
        if (changeMyCard)
            myCard = myCardInstance;

        gsm = GlobalStatsManager.Instance;
        UpdateCard();
    }

    public void Clear()
    {
        myCard = null;
    }

    public void UpdateCard()
    {
        myCard.Setup(this);
    }

public float ProccessedValue()
{
    if (gsm == null)
        gsm = GlobalStatsManager.Instance;

    float bonuses = 1 + gsm.overallEffectiveness;

    // Add category-specific efficiency
    bonuses += spellCategory switch
    {
        SpellCategory.Power => gsm.offensiveEfficiency,
        SpellCategory.Protective => gsm.protectiveEfficiency,
        SpellCategory.Dexterity => gsm.dexterityEfficiency,
        _ => 0f
    };

    // Elemental bonuses
    if (attributes.Contains(SpellAttributes.Fire))
        bonuses += gsm.fireDamage;
    if (attributes.Contains(SpellAttributes.Ice))
        bonuses += gsm.iceDamage;

    // Final value calculation
    return baseValue * (1 + multiplierPerLevel * abilityLevel) * bonuses;
}


    public int GetBounces()
    {
        return bounces + gsm.additionalBounces;
    }

    public int GetCasts()
    {
        return multicastCount + gsm.additionalProjectiles;
    }
    public float GetCooldown()
    {
        return cooldownDuration - cooldownDuration*gsm.cooldownReduction;
    }
    public void LevelUp()
    {
        abilityLevel++;
        UpdateCard();
    }
}

public enum SpellAttributes
{
    Fire,
    Ice,
    Multicast,
    AOE,
    Bounce,
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

