using UnityEngine;

public class DamageArea : MonoBehaviour
{
    public Transform who;
    public float damage;
    public Collider myCollision;

    private void OnTriggerEnter(Collider other)
    {
        if (myCollision.enabled && other.TryGetComponent(out PlayerDamageable component))
        {
            component.TakeDamage(damage, who.position - component.transform.position);
            myCollision.enabled = false;
            gameObject.SetActive(false);
        }
    }
}
