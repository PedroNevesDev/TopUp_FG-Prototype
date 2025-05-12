using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shop : MonoBehaviour, IInteractable
{
    public string interactName;
    public int maxCards = 3;
    public int maxStats = 2;
    public int currentBuys = 0;
    public int currentRerolls = 0;
    public float priceIncreasePerBuy = 0.1f;
    public int pricePerReroll = 50;
    public List<SpellSO> pickedSpells = new List<SpellSO>();
    public List<Stat> pickedStats = new List<Stat>();
    ShopManager shopManager;

    public string InteractName => interactName;

    void Start()
    {
        shopManager = ShopManager.Instance;
    }

    public void OnInteract()
    {
        shopManager.Show(this);
    }

    public void OnInterrupt()
    {
        shopManager.Close();
    }
}
