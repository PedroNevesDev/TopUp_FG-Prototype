using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [Header("HP")]
    public Image hpFill;
    public Image damageFill;
    public TextMeshProUGUI hp;
    
    [Header("EXP")]
    public Image expFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI exp;

    [Header("Transition Settings")]
    [Range(0.1f, 5f)]
    public float damageTransitionSpeed = 2f;
    [Range(0.1f, 5f)]
    public float expTransitionSpeed = 1f;

    private float targetExpFill;
    private Coroutine damageCoroutine;
    private Coroutine expCoroutine;

    int floorNumber = 1;

    private UITransitionPanelController pannel;
    private LevelManager levelManager;

    public List<string> dungeonTexts = new List<string>();

    public List<string> deathTexts = new List<string>();
    public void ShowPlayerDeathUI()
    {
        List<string> newList = new List<string>
        {
            deathTexts[Random.Range(0, deathTexts.Count)]
        };

        if(pannel)
        {
            levelManager.TogglePause(true);
            pannel.ShowTransition(newList,levelManager.Restart);
        }
    }

    public void ShowDungeonMessage()
    {
        floorNumber++;
        string floorString ="Floor "+ floorNumber;
        List<string> newList = new List<string>
        {
            dungeonTexts[Random.Range(0, dungeonTexts.Count)],
            floorString,
        };
        
        if(pannel)
        {
            levelManager.TogglePause(true);
            pannel.ShowTransition(newList,DungeonGenerator.Instance.Regenerate);
        }
    }

    public void ResetTransition()
    {

        StartCoroutine(pannel.FadeFromBlack());
    }

    public void Unpause()
    {
        levelManager.TogglePause(false);
    }
    private void Start()
    {
        pannel = GetComponent<UITransitionPanelController>();
        levelManager = LevelManager.Instance;
        // Initialize fills
        if (hpFill) hpFill.fillAmount = 1f;
        if (damageFill) damageFill.fillAmount = 1f;
        if (expFill) expFill.fillAmount = 0f;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        // Instantly change hpFill to current health
        hpFill.fillAmount = currentHealth / maxHealth;
        hp.text = currentHealth + "/" + maxHealth;

        // Stop any ongoing damage transition
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        // Smoothly decrease the damageFill to current health
        damageCoroutine = StartCoroutine(SmoothDamageTransition(currentHealth / maxHealth));
    }

    private IEnumerator SmoothDamageTransition(float targetFill)
    {
        // Wait a small delay before starting the transition
        yield return new WaitForSeconds(0.2f);

        float currentFill = damageFill.fillAmount;
        
        // Ensure we're only decreasing the fill (for damage effect)
        if (targetFill >= currentFill)
        {
            damageFill.fillAmount = targetFill;
            yield break;
        }

        // Smoothly transition to the target fill amount
        while (currentFill > targetFill)
        {
            currentFill -= Time.deltaTime * damageTransitionSpeed;
            
            // Ensure we don't go below the target
            if (currentFill <= targetFill)
                currentFill = targetFill;
                
            damageFill.fillAmount = currentFill;
            yield return null;
        }
    }

    public void UpdateExp(float currentExp, float neededExpToLevelUp)
    {
        // Update the exp text immediately
        exp.text = currentExp.ToString("F1") + "/" + neededExpToLevelUp.ToString("F1");
        
        // Calculate the target fill amount
        targetExpFill = Mathf.Clamp01(currentExp / neededExpToLevelUp);  // Ensure it's between 0 and 1
        
        // Stop any ongoing exp transition
        if (expCoroutine != null)
            StopCoroutine(expCoroutine);
            
        // Smoothly increase the expFill
        expCoroutine = StartCoroutine(SmoothExpTransition());
        
        // Debug output to verify values
        Debug.Log($"EXP Update - Current: {currentExp}, Needed: {neededExpToLevelUp}, Target Fill: {targetExpFill}");
    }

    private IEnumerator SmoothExpTransition()
    {
        float currentFill = expFill.fillAmount;
        float startingFill = currentFill;
        float duration = 1.0f; // Fixed duration for the animation
        float elapsedTime = 0f;
        
        Debug.Log($"Starting EXP transition from {startingFill} to {targetExpFill}");
        
        // Animate over a fixed duration instead of using Lerp with deltaTime
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            // Use a smoothstep for nicer easing
            float smoothT = t * t * (3f - 2f * t);
            expFill.fillAmount = Mathf.Lerp(startingFill, targetExpFill, smoothT);
            
            Debug.Log($"EXP transition progress: {expFill.fillAmount}");
            yield return null;
        }
        
        // Ensure we reach exactly the target value
        expFill.fillAmount = targetExpFill;
        Debug.Log($"EXP transition complete: {expFill.fillAmount}");
    }

    [Header("Level Up Animation")]
    public Color levelUpHighlightColor = Color.yellow;
    [Range(0.1f, 2f)]
    public float levelUpDuration = 0.5f;
    [Range(1.1f, 2f)]
    public float levelUpScaleMultiplier = 1.5f;
    private Coroutine levelAnimCoroutine;

private Queue<int> levelUpQueue = new Queue<int>();
private bool isAnimatingLevel = false;

public void UpdateLevel(int level)
{
    // Prevent duplicates and avoid level-ups from queuing too quickly
    if (isAnimatingLevel) return;

    if (levelUpQueue.Count == 0 || levelUpQueue.Peek() != level)
        levelUpQueue.Enqueue(level);

    if (!isAnimatingLevel)
        StartCoroutine(ProcessLevelQueue());
}

private IEnumerator ProcessLevelQueue()
{
    isAnimatingLevel = true;

    while (levelUpQueue.Count > 0)
    {
        int currentLevel = levelUpQueue.Dequeue();
        yield return AnimateLevelChange(currentLevel);

        // Add a small delay between level-ups (adjust the time as needed)
        yield return new WaitForSeconds(0.5f); // 0.5 seconds delay
    }

    isAnimatingLevel = false;
}



    private IEnumerator AnimateLevelChange(int newLevel)
    {
        // Cache the original values
        Vector3 originalScale = levelText.transform.localScale;
        Color originalColor = levelText.color;
        
        // Update the text
        levelText.text = newLevel.ToString();
        
        // Animation - Scale up and change color
        float elapsed = 0f;
        while (elapsed < levelUpDuration / 2)
        {
            float t = elapsed / (levelUpDuration / 2);
            
            // Scale up
            levelText.transform.localScale = Vector3.Lerp(originalScale, originalScale * levelUpScaleMultiplier, t);
            
            // Change color
            levelText.color = Color.Lerp(originalColor, levelUpHighlightColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Animation - Scale back down and restore color
        elapsed = 0f;
        while (elapsed < levelUpDuration / 2)
        {
            float t = elapsed / (levelUpDuration / 2);
            
            // Scale down
            levelText.transform.localScale = Vector3.Lerp(originalScale * levelUpScaleMultiplier, originalScale, t);
            
            // Restore color
            levelText.color = Color.Lerp(levelUpHighlightColor, originalColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're back to original values when done
        levelText.transform.localScale = originalScale;
        levelText.color = originalColor;

        GlobalStatsManager.Instance.GeneratePicks();
    }
    
    // Reset values if needed (for example when player dies or game restarts)
    public void ResetUI(float maxHealth, float currentExp, float neededExp, int level)
    {
        // Stop any ongoing transitions
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        if (expCoroutine != null)
            StopCoroutine(expCoroutine);
        if (levelAnimCoroutine != null)
            StopCoroutine(levelAnimCoroutine);
            
        // Reset health
        hpFill.fillAmount = 1f;
        damageFill.fillAmount = 1f;
        hp.text = maxHealth + "/" + maxHealth;
        
        // Reset exp
        expFill.fillAmount = currentExp / neededExp;
        targetExpFill = currentExp / neededExp;
        exp.text = currentExp.ToString("F1") + "/" + neededExp.ToString("F1");
        
        // Reset level without animation
        levelText.text = level.ToString();
        levelText.transform.localScale = Vector3.one;
        levelText.color = Color.white; // Or your default text color
    }
    
    // Test method to visualize EXP animation - can be called from editor button or test script
    public void TestExpAnimation(float startExp, float endExp, float maxExp)
    {
        // Update exp text and start with initial value
        expFill.fillAmount = startExp / maxExp;
        
        // Then animate to the new value
        UpdateExp(endExp, maxExp);
    }
}