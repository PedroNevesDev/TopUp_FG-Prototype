using NUnit.Framework;
using UnityEngine;


public class FireballSpell : ActiveSpell
{
protected override void Cast(Vector3 spawnPoint, Vector3 direction)
{
    base.Cast(spawnPoint, direction);

    if (myCard.onCooldown) return;

    Debug.Log("Casting " + spell.spellName);

    int count = spell.projectileCount + globalStats.additionalProjectiles;
    float angle = spell.projectileMaxAngle;

    for (int i = 0; i < count; i++)
    {
        float spreadStep = (count > 1) ? angle / (count - 1) : 0f;
        float currentAngle = -angle / 2f + spreadStep * i;

        Quaternion rotationOffset = Quaternion.AngleAxis(currentAngle, Vector3.up); // rotate around Y axis
        Vector3 rotatedDirection = rotationOffset * direction;

        ObjectPool.Instance.GetObject(spell.spellPrefab, spawnPoint, Quaternion.LookRotation(rotatedDirection));
    }

    StartCooldown();
}
}
