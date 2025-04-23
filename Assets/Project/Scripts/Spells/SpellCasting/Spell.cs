using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Spell : MonoBehaviour
{
    public Card myCard;
    public SpellSO spell;

    void Start()
    {
        myCard.Setup(spell);   
    }
    public virtual void Cast(Vector3 position, Vector3 direction)
    {
        
    }
}
