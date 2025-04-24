using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/NewSpell")]
public class SpellSO : ScriptableObject
{
    public List<SpellAttributes> attributes = new List<SpellAttributes>();
    public string spellName;
    public string spellDescription;
    public Sprite spellIcon;
    public float cooldownDuration = 5f;  // Example cooldown time in seconds
    [Header("Projectile")]

    public int projectileCount = 1;
    public int projectileMaxAngle = 90;

    [Header("Duration")]
    public float spellDuration = 0f;
    public bool canBeCanceled = false;

    [Header("Object")]
    public GameObject spellPrefab; // Prefab of the projectile
}
public enum SpellAttributes
{
    Power,
    Dexterity,
    Utility,
    Protective,
    Fire,
    Ice,
    Projectile,
    AOE,
    Chain,
    Duration,
}

