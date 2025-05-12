using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : Singleton<ShopManager>
{
    public PlayerDamageable playerDamageable;
    public int moneys=0;

    public GameObject shopPannel;

    public TextMeshProUGUI rerollPrice;

    UIManager uiManager;

    GlobalStatsManager globalStatsManager;
    Shop currentShop;

    public Transform spellsTransform;
    public Transform statsContent;

    public CardShop cardShopPrefab;


    float cheatTimer=2f;
    float timer=0;

    List<CardShop> shopCards = new List<CardShop>();
    void Start()
    {
        globalStatsManager = GlobalStatsManager.Instance;
        uiManager = UIManager.Instance;
        uiManager.UpdateCurrecny(moneys);
    }

    public void AddMoney(int moremoney)
    {
        moneys+=moremoney;
        uiManager.UpdateCurrecny(moneys);
    }
    void Update()
    {
        if(Input.GetKey(KeyCode.Space)&&Input.GetKey(KeyCode.LeftShift)&&Input.GetKey(KeyCode.LeftControl))
        {
            timer+=Time.deltaTime;
            if(timer>=cheatTimer)
            {
                moneys+=250;
                uiManager.UpdateCurrecny(moneys);
            }
        }
        else
        {
            timer=0;
        }
    }
    void UpdateCurrency()
    {
        uiManager.UpdateCurrency();
    }
    public void CheckMoney(int ammount)
    {
        moneys = ammount;
    }

    
    public List<SpellSO> GenerateCards(int cardCount)
    {
        return globalStatsManager.GetRandomCards(cardCount);
    }

    public List<Stat> GenerateStats(int cardCount)
    {
        return globalStatsManager.GetRandomStats(cardCount);
    }
    public bool IsShopActive()
    {
        return currentShop !=null;
    }
    public void Show(Shop shop)
    {
        if(globalStatsManager.IsPicksActive())return;

        currentShop = shop;
        if(currentShop.pickedSpells.Count == 0 && currentShop.pickedStats.Count == 0)
        {
            currentShop.pickedSpells = GenerateCards(currentShop.maxCards);
            currentShop.pickedStats = GenerateStats(currentShop.maxStats);
            Debug.LogError("GENERATED");
        }
        UpdateRerollCards();

        UpdateReroll(currentShop.currentRerolls * currentShop.pricePerReroll + currentShop.pricePerReroll);
        shopPannel.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(statsContent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(spellsTransform.GetComponent<RectTransform>());
    }

    public void UpdateReroll(int price)
    {
        rerollPrice.text = price.ToString();
    }

public void UpdateRerollCards()
{
    if (currentShop == null) return;

    // Clear previous cards
    shopCards.ForEach(s => Destroy(s.gameObject));
    shopCards.Clear();

    // Create and set up spell cards
    foreach (var p in currentShop.pickedSpells)
    {
        CardShop newCard = Instantiate(cardShopPrefab, spellsTransform)
            .Setup(p, currentShop.currentBuys * currentShop.priceIncreasePerBuy);
        newCard.AddComponent<Selectable>().Setup(() => Buy(newCard));
        shopCards.Add(newCard);
    }

    // Create and set up stat cards
    foreach (var p in currentShop.pickedStats)
    {
        CardShop newCard = Instantiate(cardShopPrefab, statsContent)
            .Setup(p, currentShop.currentBuys * currentShop.priceIncreasePerBuy);
        newCard.AddComponent<Selectable>().Setup(() => Buy(newCard));
        shopCards.Add(newCard);
    }
}

    void UpdateCardPrices()
    {
        if(currentShop ==null)return;
        shopCards.ForEach(s=>s.UpdatePrice(currentShop.currentBuys*currentShop.priceIncreasePerBuy));
    }

    public void Buy(CardShop cardShop)
    {
        if(currentShop == null)return;
        if(cardShop.GetCardPrice(currentShop.currentBuys*currentShop.priceIncreasePerBuy)>moneys)return;

        moneys -= cardShop.GetCardPrice(currentShop.currentBuys*currentShop.priceIncreasePerBuy);
        cardShop.Apply();
        shopCards.Remove(cardShop);
        Destroy(cardShop.gameObject);
        currentShop.currentBuys++;
        UpdateCardPrices();
        uiManager.UpdateCurrecny(moneys);
        
    }
    public void Reroll()
    {
        if(currentShop==null)return;
        if(moneys< currentShop.currentRerolls*currentShop.pricePerReroll+currentShop.currentRerolls)return;

        moneys -= currentShop.currentRerolls*currentShop.pricePerReroll+currentShop.currentRerolls;
        uiManager.UpdateCurrecny(moneys);
        currentShop.currentRerolls++;
        currentShop.pickedSpells = GenerateCards(currentShop.maxCards);
        currentShop.pickedStats = GenerateStats(currentShop.maxStats);
        UpdateRerollCards();
    }
    public void Close()
    {
        currentShop = null;
        shopPannel.SetActive(false);
    }
    public void Heal()
    {
        if(moneys<100)return;

        moneys-=100;
        playerDamageable.Heal(50);
    }
}
