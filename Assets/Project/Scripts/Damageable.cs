using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Damageable : MonoBehaviour
{
    [Header("Color Change")]
    public Color targetColor;
    Color initialColor;

    protected float slow = 0;
    protected float controlableResist = 0;
    public MeshRenderer meshRenderer;
    [Header("Health Related")]
    public float health;
    public float baseHealth = 70f; 
    public float maxHealth;
    public Image healthBar;
    public Image damageBar;

    public CameraManager cameraManager;
    public bool isAffectedByKnockback;
    [Header("Bar Fade Settings")]
    public float barFadeOut = 2f;
    public float barFadeIn = 0.2f;

    public CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private ShakeEffect shakeEffect;
    ObjectPool objectPool;
    protected GlobalStatsManager globalStatsManager;
    Rigidbody rb;

    public List<DropData> drops = new List<DropData>();

    [System.Serializable]
    public class DropData
    {
        public GameObject drop;
        public int min, max;

        public void Drop(Vector3 pos)
        {
            int quantity = Random.Range(min,max+1);
            for(int i = 0;i<quantity;i++)
            {
                ObjectPool.Instance.GetObject(drop,pos,Quaternion.identity).GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f)),ForceMode.Impulse);
            }
        }
    }
    void Start()
    {
        initialColor = meshRenderer.sharedMaterial.color;
        cameraManager = CameraManager.Instance;
        rb = GetComponent<Rigidbody>();
        objectPool = ObjectPool.Instance;
        globalStatsManager = GlobalStatsManager.Instance;
        shakeEffect = GetComponentInChildren<ShakeEffect>();
    }

    public void ApplyEffect(EffectType effectType,float value, float duration)
    {
        switch(effectType)
        {
            case EffectType.Burn:
            break;
            case EffectType.Frost:
            StopCoroutine(Frost(value,duration));
            StartCoroutine(Frost(value,duration));
            break;
            case EffectType.Eletracute:
            break;
        }
    }

    IEnumerator Frost(float slowValue,float duration)
    {
        slow = slowValue;
        DamageNumber dmg = objectPool.GetObject(globalStatsManager.damageNumberPrefab,transform.position+new Vector3(0,3,0),Quaternion.identity).GetComponent<DamageNumber>();
        dmg.Setup("<color=blue>FREEZE</color>");
        yield return new WaitForSeconds(duration);
        slow = 0;
    }

    
    protected virtual void OnEnable()
    {
        if(globalStatsManager==null)globalStatsManager = GlobalStatsManager.Instance;
        maxHealth = baseHealth+globalStatsManager.GetHealth();
        if (canvasGroup == null)
        {
            Debug.LogError("Missing CanvasGroup on health bar parent.");
        }
        canvasGroup.alpha = 0; // Start invisible
        health = baseHealth + globalStatsManager.enemyHealthPerLevel*globalStatsManager.floor;
        maxHealth = health;
        UpdateBarInstant();
    }

    public void TakeDamage(float damage, Vector3 dirKnockback, float knockbackForce)
    {
        float proccessedDamage = (damage - (globalStatsManager.GetResist()*damage))*controlableResist;

        health -= proccessedDamage;
        DamageNumber dmg = objectPool.GetObject(globalStatsManager.damageNumberPrefab,transform.position+new Vector3(0,3,0),Quaternion.identity).GetComponent<DamageNumber>();
        cameraManager.ShakeActiveCamera(0.5f,dirKnockback);
        dmg.Setup(proccessedDamage.ToString("F1"));
        if(isAffectedByKnockback)
        {
            rb.AddForce(dirKnockback.normalized*knockbackForce,ForceMode.Impulse);
        }
        CheckHealth();
        UpdateBar();
        if (shakeEffect)
        {
            shakeEffect.Shake();
        }
        if(health==0)
        {
            drops.ForEach(d=>d.Drop(transform.position+new Vector3(0,0.5f,0)));
            StopAllCoroutines();
            if(this is Enemy)
            {
                DungeonGenerator.Instance.RemoveEnemy(this as Enemy);
            }
            objectPool.ReturnObject(gameObject);
        }
    }
    IEnumerator ColorChange()
    {
        meshRenderer.sharedMaterial.color = targetColor;
        yield return new WaitForSeconds(0.2f);
        meshRenderer.sharedMaterial.color = initialColor;
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

public enum EffectType
{
    None,
    Burn,
    Frost,
    Eletracute
}