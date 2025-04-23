using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [Header("Card References")] 
    public TextMeshProUGUI cardTitle;
    public TextMeshProUGUI cardDescription;
    public Image spellImage;
    public Image darkning;
    public Image cdImage;

    public TextMeshProUGUI cdText;

    float currentCD;

    private SpellSO spellSO;
    public bool onCooldown = false;

    public bool onDuration = false;

    float currentDuration;
    public void Setup(SpellSO spellData)
    {
        SetText(spellData.cooldownDuration);
        spellSO = spellData;
        cardTitle.text = spellData.spellName;
        cardDescription.text = spellData.spellDescription;
        spellImage.sprite = spellData.spellIcon;
    }
    void FixedUpdate()
    {
        if(onCooldown)
        {
            currentCD-= Time.fixedDeltaTime;
            cdImage.fillAmount = currentCD/spellSO.cooldownDuration;
            SetText(currentCD);
            if(currentCD<=0f)
            {
                onCooldown = false;
                SetText(spellSO.cooldownDuration);
                darkning.enabled = false;
                cdImage.fillAmount = 1f;
            }            
        }

        if(onDuration)
        {
            currentDuration -= Time.fixedDeltaTime;
            cdImage.fillAmount -= currentDuration/spellSO.spellDuration;
            SetText(currentDuration);
            if(currentDuration<=0f)
            {
                onDuration = false;
                SetText(spellSO.spellDuration);
                cdImage.fillAmount = 1f;
                StartCooldown();
            }
        }
    }
    void SetText(float num)
    {
        cdText.text = Mathf.RoundToInt(num).ToString();
    }

    public void StartCooldown()
    {
        currentCD = spellSO.cooldownDuration;
        onCooldown = true;
        darkning.enabled = true;
    }

    public void StartDuration()
    {
        currentDuration = spellSO.spellDuration;
        onDuration = true;
    }
}
