
using UnityEngine;

[CreateAssetMenu(fileName = "Stat", menuName = "Scriptable Objects/NewStat")]
public class Stat : ScriptableObject
{
    public StatDisplayMode displayMode;
    public StatType type;

    public Sprite statIcon;

    public float minValue = 0;
    public float maxValue = 1;

    public float chanceOfDropping = 0;

    public float currentValue = 0;

    [Tooltip("Use {0} to insert value. Example: '+{0} Multicast'")]
    public string format = "+{0} {1}";

    public string smallDescription = "Description";
    public void RollValue()
    {
        currentValue = Random.Range(minValue, maxValue);
    }

    public string GetFormatted()
    {
        string val;

        if (displayMode == StatDisplayMode.Percentage)
        {
            val = (currentValue * 100f).ToString("0.#") + "%";
        }
        else
        {
            val = currentValue % 1 == 0 ? ((int)currentValue).ToString() : currentValue.ToString("0.##");
        }

        return string.Format(format, val, type.ToString());
    }

}
public enum StatDisplayMode
{
    Flat,       // e.g., +1 Multicast
    Percentage  // e.g., +10% Cooldown Reduction
}

public enum StatType
{
    FireDamage,
    IceDamage,
    Bounces,
    Multicast,
    ShopDiscount,
    AttackSpeed,
    CooldownReduction,
    Power,
    Dexterity,
    Protective,
    Health,
}
