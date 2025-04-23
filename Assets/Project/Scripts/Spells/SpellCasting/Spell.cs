using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Spell : MonoBehaviour
{
    public Card myCard;
    public SpellSO spell;

    public GlobalStatsManager globalStats;
    void Start()
    {
        myCard.Setup(spell);
        globalStats = GlobalStatsManager.Instance;
    }
    public virtual void Cast(Vector3 position, Vector3 direction)
    {
        
    }

    public virtual void AfflictUser()
    {
        PlayerController player = PlayerController.Instance;

        //Affect Player
    }
}
