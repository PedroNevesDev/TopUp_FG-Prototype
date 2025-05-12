using System.Collections.Generic;
using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            GlobalStatsManager.Instance.GenerateRandomCard();
            Destroy(gameObject);
        }
    }
}
