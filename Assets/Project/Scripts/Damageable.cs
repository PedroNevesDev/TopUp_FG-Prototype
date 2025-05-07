using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Damageable : MonoBehaviour
{
    [Header("Health Related")]
    public float health;
    public float maxHealth = 100f;
    public Image healthBar;
    public bool isAffectedByKnockback;
    [Header("Bar Fade Settings")]
    public float barFadeOut = 2f;
    public float barFadeIn = 0.2f;

    public CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private ShakeEffect shakeEffect;
    ObjectPool objectPool;
    GlobalStatsManager globalStatsManager;
    Rigidbody rb;

    public List<GameObject> drops = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectPool = ObjectPool.Instance;
        globalStatsManager = GlobalStatsManager.Instance;
        shakeEffect = GetComponentInChildren<ShakeEffect>();
        if (canvasGroup == null)
        {
            Debug.LogError("Missing CanvasGroup on health bar parent.");
        }
        UpdateBarInstant();
        canvasGroup.alpha = 0; // Start invisible
    }

    public void TakeDamage(float damage, Vector3 dirKnockback)
    {
        float proccessedDamage = damage - (globalStatsManager.enemyResist*damage);
        health -= proccessedDamage;
        DamageNumber dmg= objectPool.GetObject(globalStatsManager.damageNumberPrefab,transform.position+new Vector3(0,2,0),Quaternion.identity).GetComponent<DamageNumber>();
        dmg.Setup(proccessedDamage.ToString());
        if(isAffectedByKnockback)
        {
            rb.AddForce(dirKnockback,ForceMode.Impulse);
        }
        CheckHealth();
        UpdateBar();
        if (shakeEffect)
        {
            shakeEffect.Shake();
        }
        if(health==0)
        {
            objectPool.ReturnObject(gameObject);
        }
    }

    public void Heal(float heal)
    {
        health += heal;
        CheckHealth();
        UpdateBar();
    }

    void CheckHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    void UpdateBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeBar());
    }

    void UpdateBarInstant()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }

    IEnumerator FadeBar()
    {
        // Fade in quickly
        yield return StartCoroutine(FadeCanvasGroup(1f, barFadeIn));

        // Wait for a while before fading out
        yield return new WaitForSeconds(1f);

        // Fade out slowly
        yield return StartCoroutine(FadeCanvasGroup(0f, barFadeOut));
    }

    IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}