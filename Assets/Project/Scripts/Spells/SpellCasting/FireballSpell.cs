using NUnit.Framework;
using UnityEngine;


public class FireballSpell : ActiveSpell
{
    public override void Cast(Vector3 spawnPoint, Vector3 direction )
    {
        base.Cast( spawnPoint, direction );

        if(myCard.onCooldown) return;
        
        Debug.Log("Casting " + spell.spellName);

        ObjectPool.Instance.GetObject(spell.spellPrefab, spawnPoint, Quaternion.LookRotation(direction));

        StartCooldown();
    }
}
