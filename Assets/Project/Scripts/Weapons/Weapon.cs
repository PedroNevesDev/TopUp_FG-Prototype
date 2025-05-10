using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public LayerMask whatToHit;
    public List<AnimData> attackAnimations = new List<AnimData>();
    
    private GlobalStatsManager gsm;
    private Animator cachedAnimator;
    private Dictionary<string, AnimData> animDictionary = new Dictionary<string, AnimData>();
    
    // Combo state
    private int currentAttackIndex = 0;
    private bool canAttack = true;
    private bool comboAvailable = false;
    private float comboWindow = 0.4f; // Adjust this in Inspector
    private float comboWindowTimer;

    public Weapon dualWield;

    bool canDamage = false;

    void Start()
    {
        gsm = GlobalStatsManager.Instance;
        foreach (var anim in attackAnimations)
        {
            animDictionary.Add(anim.animation.name, anim);
            // Initialize all colliders as disabled
            foreach (var col in anim.colliders)
            {
                col.enabled = false;
            }
        }
    }

    void Update()
    {
        if (comboAvailable)
        {
            comboWindowTimer -= Time.deltaTime;
            if (comboWindowTimer <= 0)
            {
                comboAvailable = false;
                OnAttackComplete();
            }
        }
    }

    public void Attack(Animator animator)
    {
        if (!canAttack)
        {
            if (comboAvailable)
            {
                ContinueCombo(animator);
            }
            return;
        }
        animator.SetFloat("AttackSpeed",weaponData.attackSpeed*gsm.attackSpeed);
        StartNewAttack(animator);

        if(dualWield)
        {
            dualWield.canDamage = true;
        }
        canDamage = true;
    }

    private void StartNewAttack(Animator animator)
    {
        currentAttackIndex = 1; // Start with first attack
        canAttack = false;
        SetAnimator(animator);
        if(dualWield)
        dualWield.SetAnimator(animator);
        animator.SetInteger("Attack", currentAttackIndex);
    }

    private void ContinueCombo(Animator animator)
    {
        currentAttackIndex++;
        if (currentAttackIndex > attackAnimations.Count)
        {
            currentAttackIndex = 1; // Loop back to first attack
        }
        
        SetAnimator(animator);
        if(dualWield)
        dualWield.SetAnimator(animator);
        animator.SetInteger("Attack", currentAttackIndex);
        comboAvailable = false; // Reset until next window opens
    }

    // Called via Animation Event when combo can be chained
    public void OpenComboWindow()
    {
        comboAvailable = true;
        comboWindowTimer = comboWindow;
    }

    // Called via Animation Event when attack fully completes
    public void OnAttackComplete()
    {
        currentAttackIndex = 0;
        canAttack = true;
        comboAvailable = false;
        
        if (cachedAnimator != null)
        {
            cachedAnimator.SetInteger("Attack", 0);
            DisableAllColliders();
        }
    }

    public void ToggleColliders(bool state)
    {


        AnimatorStateInfo stateInfo = cachedAnimator.GetCurrentAnimatorStateInfo(0);
        string clipName = GetCurrentAnimationName(cachedAnimator);
        
        if (animDictionary.TryGetValue(clipName, out AnimData animData))
        {
            foreach (var col in animData.colliders)
            {
                col.enabled = state;
            }
            
            foreach (var trail in animData.trailRenderers)
            {
                trail.emitting = false;
                trail.Clear();
                trail.emitting = true;
                trail.enabled = state;
            }
        }
    }

    private string GetCurrentAnimationName(Animator animator)
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        return clipInfo.Length > 0 ? clipInfo[0].clip.name : "";
    }

    private void DisableAllColliders()
    {
        foreach (var anim in attackAnimations)
        {
            foreach (var col in anim.colliders)
            {
                col.enabled = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Damageable target)&&canDamage)
        {
            canDamage = false;
            target.TakeDamage(weaponData.weaponDamage * gsm.physicalEfficiency,weaponData.hasKnockback?(other.transform.position-cachedAnimator.transform.position).normalized*weaponData.knockback:Vector3.zero);
            StopCoroutine(ApplyHitStop());
            StartCoroutine(ApplyHitStop());
        }
    }
    public void SetAnimator(Animator animator)
    {
        cachedAnimator = animator;
    }
    
    public bool IsAttacking()
    {
        return currentAttackIndex != 0;
    }

    IEnumerator ApplyHitStop()
    {
        cachedAnimator.speed = 0;
        yield return new WaitForSeconds(0.15f);
        cachedAnimator.speed = 1;
    }
}
[System.Serializable]
public class AnimData
{
    public AnimationClip animation;

    public List<TrailRenderer> trailRenderers;
    public List<Collider> colliders = new List<Collider>(); // Names of colliders to activate

    public float attackSpeed;
}
