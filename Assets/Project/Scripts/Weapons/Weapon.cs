using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public TrailRenderer trailRenderer;
    private List<Collider> attackColliders = new List<Collider>();

    public LayerMask whatToHit;

    private GlobalStatsManager gsm;
    public Animator cachedAnimator;

    void Start()
    {
        attackColliders = GetComponents<Collider>().ToList();
        gsm = GlobalStatsManager.Instance;
    }
    public void Attack(Animator animator)
    {
        weaponData.PlayAttack(animator);
        animator.SetBool("IsAttacking", true);
        cachedAnimator = animator;
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
        attackColliders.ForEach(col=>col.enabled = state);
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        trailRenderer.emitting = true;
        trailRenderer.enabled = state;
    }
    public void OnAttackEnd()
    {
        if(!cachedAnimator)return;
        cachedAnimator.SetBool("IsAttacking", false);
        cachedAnimator = null;
    }

}
