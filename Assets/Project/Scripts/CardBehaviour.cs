using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    public GameObject myCard;
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            CardManager.Instance.AddCard(myCard);
            Destroy(gameObject);
        }
    }
}
