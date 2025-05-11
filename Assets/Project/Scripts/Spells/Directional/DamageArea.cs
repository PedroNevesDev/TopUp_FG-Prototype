using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public Transform who;
    public float damage;

    public Collider myCollision;

    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerDamageable component))
        {
            myCollision.enabled = false;
            gameObject.SetActive(false);
            component.TakeDamage(damage,who.position-component.transform.position);
        }
    }
}
