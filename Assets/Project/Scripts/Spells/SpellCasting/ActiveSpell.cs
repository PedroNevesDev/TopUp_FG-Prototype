using System;
using UnityEngine;

public class ActiveSpell : Spell
{
    protected override void Cast(Vector3 spawnPoint, Vector3 direction)
    {
        if (myCard.onCooldown)
        {
            Debug.Log(spell.spellName + " is on cooldown!");
            return;
        }

        // Cast the spell (spawn the fireball, etc.)
        Debug.Log(spell.spellName + " cast!");
    }

    protected override void AfflictUser(PlayerController target)
    {
        if(myCard.onDuration)
        {
            Debug.Log(spell.spellName + " is already Active!");
            return;            
        }
        if (myCard.onCooldown)
        {
            Debug.Log(spell.spellName + " is on cooldown!");
            return;
        }

        // Cast the spell (spawn the fireball, etc.)
        Debug.Log(spell.spellName + " cast!");
    }

    protected void StartCooldown()
    {
        myCard.StartCooldown();
    }
}
