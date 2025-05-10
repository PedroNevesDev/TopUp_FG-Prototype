using UnityEngine;

[System.Serializable]
public class Stat
{
    public StatType type;

    public float minValue = 0;
    public float maxValue = 10;

    public float currentValue = 0;

    [Tooltip("Use {0} to insert value. Example: '+{0} Multicast'")]
    public string format = "+{0} {1}";

    public string GetFormatted()
    {
        string val = currentValue % 1 == 0 ? ((int)currentValue).ToString() : currentValue.ToString("0.##");
        return string.Format(format, val, type.ToString());
    }
}
public enum StatType
{
    FireDamage,
    Bounces,
    Multicast,
    ShopDiscount
}
