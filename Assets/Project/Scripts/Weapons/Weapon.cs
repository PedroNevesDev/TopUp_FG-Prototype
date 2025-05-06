using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public TrailRenderer trailRenderer;
    private List<Collider> attackColliders = new List<Collider>();

    public LayerMask whatToHit;

    private GlobalStatsManager gsm;
    public Animator cachedAnimator;
    public List<AnimData> attackAnimations = new List<AnimData>();

    Dictionary<string, AnimData> animDictionary = new Dictionary<string, AnimData>();


    int currentAttackIndex = 0;
    bool canAttack;

    bool nextAttack;

    void Start()
    {
        attackColliders = GetComponents<Collider>().ToList();
        gsm = GlobalStatsManager.Instance;
        attackAnimations.ForEach(anim =>{
            animDictionary.Add(anim.animation.name, anim);
        });
    }
public void Attack(Animator animator)
{
    if (IsAttacking())
    {
        // Request combo continuation
        nextAttack = true;
        return;
    }

    currentAttackIndex = 1;
    animator.SetInteger("Attack", currentAttackIndex);
    cachedAnimator = animator;
    canAttack = false;
}
    
    void Next()
    {
        currentAttackIndex++;
        if(currentAttackIndex >= attackAnimations.Count)
        {
            currentAttackIndex = 1;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Damageable target))
        {
            target.TakeDamage(weaponData.weaponDamage*gsm.physicalEfficiency);
        }
    }
    public void ToggleColliders(bool state)
    {
        if(!cachedAnimator)return;
        string currentAnimationName = cachedAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        AnimData requestedData = animDictionary[currentAnimationName];
        requestedData.colliders.ForEach(col=>col.enabled = state);
        requestedData.trailRenderers.ForEach(trail=>{
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            trailRenderer.emitting = true;
            trailRenderer.enabled = state;
        });

    }
public void OnAttackEnd()
{
    

    if (nextAttack)
    {
        nextAttack = false;
        currentAttackIndex++;
        if (currentAttackIndex >= attackAnimations.Count)
        {
            currentAttackIndex = 1; // Wrap combo to first attack
        }
        cachedAnimator.SetInteger("Attack", currentAttackIndex);
    }
    else
    {
        currentAttackIndex = 0;
        cachedAnimator.SetInteger("Attack", 0);
        canAttack = true;
        cachedAnimator = null;
    }
}
    public bool IsAttacking()
    {
        return currentAttackIndex !=0;
    }
}
[System.Serializable]
public class AnimData
{
    public AnimationClip animation;

    public List<TrailRenderer> trailRenderers;
    public List<Collider> colliders = new List<Collider>(); // Names of colliders to activate
}
