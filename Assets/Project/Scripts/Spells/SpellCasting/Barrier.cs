using UnityEngine;

public class BarrierSpell : ActiveSpell
{
    public override void AfflictUser()
    {
        base.AfflictUser();
        if(myCard.onDuration) return;
        if(myCard.onCooldown) return;
        
        Debug.Log("Casting " + spell.spellName);

        ObjectPool.Instance.GetObject(spell.spellPrefab);

        StartCooldown();
    }
}
