using UnityEngine;

public class BarrierSpell : ActiveSpell
{
    protected override void AfflictUser(PlayerController target)
    {
        base.AfflictUser(target);
        if(myCard.onDuration) return;
        if(myCard.onCooldown) return;
        
        Debug.Log("Casting " + spell.spellName);

        ObjectPool.Instance.GetObject(spell.spellPrefab);

        StartCooldown();
    }
}
