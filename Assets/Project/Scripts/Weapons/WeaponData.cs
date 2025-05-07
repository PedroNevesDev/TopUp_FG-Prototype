using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float weaponDamage = 45;
    public bool hasKnockback = false;
    public float knockback = 2f;
}
