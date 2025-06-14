using System.Collections;
using UnityEngine;

public class BasicSpellTriggerDamage : MonoBehaviour
{

    void OnEnable()
    {
        StartCoroutine(WaitToReturn());
    }
    IEnumerator WaitToReturn()
    {
        yield return new WaitForSeconds(returnTimer);
        ObjectPool.Instance.ReturnObject(gameObject);
    }
    public float returnTimer;

    public SpellSO spellSO;
    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")&&other.TryGetComponent(out Damageable component))
        {
            component.TakeDamage(spellSO.ProccessedValue(),Vector3.up,0.1f);
            spellSO.CheckIfShouldApplyEffect(component);
        }
    }
}
