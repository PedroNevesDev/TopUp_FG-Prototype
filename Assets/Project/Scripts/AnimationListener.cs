using UnityEngine;

public class AnimationListener : MonoBehaviour
{
public PlayerController player;

    public void EnableWeaponColliders()
    {
        if(player.currentWeapon == null)return;

        player.currentWeapon.ToggleColliders(true);
    }
    public void DisableWeaponColliders()
    {
        if(player.currentWeapon == null)return;

        player.currentWeapon.ToggleColliders(false);
    }
}
