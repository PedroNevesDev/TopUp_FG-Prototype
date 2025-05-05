using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float weaponDamage = 45;
    public List<AnimData> attackAnimations = new List<AnimData>();

    int currentIndex = -1;

    public void PlayAttack(Animator animator)
    {
        if (attackAnimations == null || attackAnimations.Count==0)
            return;

        currentIndex++;
        if(currentIndex >= attackAnimations.Count)
        currentIndex = 0;
        
        var animData = attackAnimations[currentIndex];

        animator.speed = animData.animationSpeed;
        animator.Play(animData.animation.name);
    }
}
[System.Serializable]
public struct AnimData
{
    public AnimationClip animation;
    public float animationSpeed;
}