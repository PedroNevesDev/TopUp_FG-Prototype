
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

    public int shopPrice = 70;
    public void RollValue()
    {
        if(displayMode==StatDisplayMode.Flat)
        {
            currentValue = Random.Range((int)minValue, (int)maxValue+1);  
        }
        else
        {
            currentValue = Random.Range(minValue, maxValue);            
        }

    }

public string GetFormatted()
{
    string val;

    if (displayMode == StatDisplayMode.Percentage)
    {
        // Display as whole percentage (no decimals)
        val = ((int)(currentValue * 100f)).ToString() + "%"; // Cast to int to avoid decimals
    }
    else
    {
        // Format value to 4 decimals max
        val = currentValue % 1 == 0 ? ((int)currentValue).ToString() : currentValue.ToString("0.####");
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
    Fire,
    Ice,
    Bounces,
    Multicast,
    Shop_Discount,
    Attack_Speed,
    Cooldown_Reduction,
    Power,
    Dexterity,
    Protective,
    Health,
    Melee_Damage,
}
