using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("HP")]
    public Image hpFill;
    public TextMeshProUGUI hp;

    [Header("EXP")]
    public Image expFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI exp;

    public void UpdateHealth(float currentHealth,float maxHealth)
    {
        hpFill.fillAmount = currentHealth/maxHealth;
        hp.text = currentHealth+"/"+maxHealth;
    }

    public void UpdateExp(float currentExp, float neededExpToLevelUp)
    {
        expFill.fillAmount=currentExp/neededExpToLevelUp;
        exp.text = currentExp+"/"+neededExpToLevelUp;
    }

    public void UpdateLevel(int level)
    {
        levelText.text = level.ToString();
    }
}

