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

    float currentTime;

    private SpellSO spellSO;
    public bool onCooldown = false;
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
            currentTime-= Time.fixedDeltaTime;
            cdImage.fillAmount = currentTime/spellSO.cooldownDuration;
            SetText(currentTime);
            if(currentTime<=0f)
            {
                onCooldown = false;
                SetText(spellSO.cooldownDuration);
                darkning.enabled = false;
                cdImage.fillAmount = 1f;
            }            
        }

    }
    void SetText(float num)
    {
        cdText.text = Mathf.RoundToInt(num).ToString();
    }

    public void StartCooldown()
    {
        currentTime = spellSO.cooldownDuration;
        onCooldown = true;
        darkning.enabled = true;
        cdImage.enabled = true;
    }
}
