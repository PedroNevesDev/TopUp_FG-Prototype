using System;
using UnityEngine;

public class ActiveSpell : Spell
{
    protected bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    public override void Cast(Vector3 spawnPoint, Vector3 direction)
    {
        if (isOnCooldown)
        {
            Debug.Log(spell.spellName + " is on cooldown!");
            return;
        }

        // Cast the spell (spawn the fireball, etc.)
        Debug.Log(spell.spellName + " cast!");
    }

    protected void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = spell.cooldownDuration;
    }

    // Update is called every frame
    public void Update()
    {
        if (isOnCooldown)
        {
            // Countdown the cooldown timer
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log(spell.spellName + " is ready to cast again!");
            }
        }
    }
}
