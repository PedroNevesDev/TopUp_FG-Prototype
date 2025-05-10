using NUnit.Framework;
using UnityEngine;


public class MulticastSpell : ActiveSpell
{
protected override void Cast(Vector3 spawnPoint, Vector3 direction)
{
    base.Cast(spawnPoint, direction);

    if (myCard.onCooldown) return;

    Debug.Log("Casting " + spell.spellName);

    int count = spell.GetCasts();
    float angle = spell.multicastMaxAngle;

        Vector3 flatDirection = direction;

        if (spell.isGrounded)
        {
            // Flatten the direction to the horizontal plane
            flatDirection.y = 0f;
            flatDirection.Normalize();
        }

        for (int i = 0; i < count; i++)
        {
            float spreadStep = (count > 1) ? angle / (count - 1) : 0f;
            float currentAngle = -angle / 2f + spreadStep * i;

            Quaternion rotationOffset = Quaternion.AngleAxis(currentAngle, Vector3.up); // horizontal only
            Vector3 rotatedDirection = rotationOffset * flatDirection;

            // No need to offset again if you've already done it outside
            ObjectPool.Instance.GetObject(spell.spellPrefab, spawnPoint, Quaternion.LookRotation(rotatedDirection));
        }

    StartCooldown();
}
}
