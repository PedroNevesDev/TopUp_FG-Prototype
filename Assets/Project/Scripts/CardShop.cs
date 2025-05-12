using TMPro;
using UnityEngine;

public class CardShop : MonoBehaviour
{
    public Card cardPrefab;
    public StatModifierCard statModifierCard;

    private Card istancedCard;
    private StatModifierCard instancedStat;
    public Transform content;
    public TextMeshProUGUI priceText;

    SpellSO mySpellSO;
    Stat myStat;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public CardShop Setup(SpellSO spellSO, float shopPriceMultiplier)
    {
        mySpellSO = spellSO;
       
        priceText.transform.localPosition = Instantiate(cardPrefab,content).Setup(spellSO).transform.localPosition+new Vector3(-5,0,0);
        UpdatePrice(shopPriceMultiplier);
        return this;
    }

    public CardShop Setup(Stat stat, float shopPriceMultiplier)
    {
        myStat = stat;
        Instantiate(statModifierCard,content).Setup(stat);
        UpdatePrice(shopPriceMultiplier);
        return this;
    }

    public void UpdatePrice(float rate)
    {
        priceText.text = GetCardPrice(rate).ToString();
    }

    public int GetCardPrice(float rate)
    {
        int price = 0;
        if(mySpellSO)
        {
            price = (int)((int)(rate*mySpellSO.shopPrice + mySpellSO.shopPrice)-(GlobalStatsManager.Instance.shopDiscount*(int)(rate*mySpellSO.shopPrice + mySpellSO.shopPrice)));
        }
        else if(myStat)
        {
            price = (int)((int)(rate*myStat.shopPrice + myStat.shopPrice)-(GlobalStatsManager.Instance.shopDiscount*(int)(rate*myStat.shopPrice + myStat.shopPrice)));
        }
        return price;
    }

    public void Apply()
    {
        if(mySpellSO)
        {
            CardManager.Instance.AddCard(mySpellSO);
        }
        else if(myStat)
        {
            GlobalStatsManager.Instance.ApplyStat(myStat);
        }
    }
}
