using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/NewSpell")]
public class SpellSO : ScriptableObject
{
    public string spellName;
    public float cooldownDuration = 5f;  // Example cooldown time in seconds
    public GameObject spellPrefab; // Prefab of the projectile
}
