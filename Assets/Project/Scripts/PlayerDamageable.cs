using UnityEngine;

public class PlayerDamageable : MonoBehaviour
{
    [Header("Health Settings")]
    public float health;
    public float maxHealth = 100f;

    public float resist = 0;

    [Header("Knockback")]
    public bool isAffectedByKnockback = true;
    private Rigidbody rb;

    [Header("References")]
    protected UIManager uiManager;
    private GlobalStatsManager globalStatsManager;
    public CameraManager cameraManager;
    private ShakeEffect shakeEffect;

    void Start()
    {
        globalStatsManager = GlobalStatsManager.Instance;
        uiManager = UIManager.Instance;
        cameraManager = CameraManager.Instance;
        rb = GetComponent<Rigidbody>();
        shakeEffect = GetComponentInChildren<ShakeEffect>();

        health = maxHealth;
        uiManager.UpdateHealth(health, maxHealth);
    }

    public void TakeDamage(float damage, Vector3 knockbackDirection)
    {
        float processedDamage = damage - (resist * damage);
        health -= processedDamage;
        health = Mathf.Clamp(health, 0, maxHealth);

        uiManager.UpdateHealth(health, maxHealth);
        //uiManager.ShowDamageEffect(processedDamage);
        print(processedDamage+ "  from resist"+resist + " X " +damage );

        cameraManager.ShakeActiveCamera(0.02f*processedDamage, knockbackDirection); // Optional screen shake
        if (shakeEffect) shakeEffect.Shake();

        if (isAffectedByKnockback && rb != null)
        {
            rb.AddForce(knockbackDirection, ForceMode.Impulse);
        }

        if (health <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        uiManager.UpdateHealth(health, maxHealth);
    }

    private void HandleDeath()
    {
        Debug.Log("Player Died");
        uiManager.ShowPlayerDeathUI();
        // Optional: disable player controls, play death animation, etc.
    }
}
