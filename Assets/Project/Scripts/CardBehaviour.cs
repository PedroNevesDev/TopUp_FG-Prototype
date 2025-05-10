using System.Collections.Generic;
using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    public List<SpellSO> listPossibleSpells = new List<SpellSO>();
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            CardManager.Instance.AddCard(listPossibleSpells[Random.Range(0,listPossibleSpells.Count)]);
            Destroy(gameObject);
        }
    }
}
