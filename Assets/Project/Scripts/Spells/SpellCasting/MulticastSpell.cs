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
    Vector3 flatDirection = direction;

    if (spell.isGrounded)
    {
        flatDirection.y = 0f;
        flatDirection.Normalize();
    }

    if (spell.useAngle)
    {
        // Spread evenly around 360°
        for (int i = 0; i < count; i++)
        {
            float currentAngle = 360f * i / count;
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 rotatedDir = rotation * flatDirection;

            ObjectPool.Instance.GetObject(spell.spellPrefab, spawnPoint, Quaternion.LookRotation(rotatedDir));
        }
    }
    else
    {
        // Dynamic small spread around forward direction
        float baseAnglePerCast = 9f; // small angle base
        float anglePerCast = Mathf.Max(1f, baseAnglePerCast - count * 0.2f); // reduce angle with more casts
        float totalAngle = anglePerCast * (count - 1);

        for (int i = 0; i < count; i++)
        {
            float currentAngle = -totalAngle / 2f + anglePerCast * i;
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 rotatedDir = rotation * flatDirection;

            ObjectPool.Instance.GetObject(spell.spellPrefab, spawnPoint, Quaternion.LookRotation(rotatedDir));
        }
    }

    StartCooldown();
}


}
