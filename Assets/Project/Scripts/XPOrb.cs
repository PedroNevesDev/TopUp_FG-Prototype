using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class XPOrb : MonoBehaviour
{
    public float minExp, maxExp;
    public float initialImpulseForce = 2f;
    public float upwardForce = 1f;
    public float homingDuration = 0.5f;

    private bool isHoming = false;

    PlayerController player;

private void OnTriggerEnter(Collider other)
{
    if (isHoming) return;

    if (other.TryGetComponent(out PlayerController player))
    {
        isHoming = true;

        // Apply impulse
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y = 0f;
        Vector3 impulse = (randomDir.normalized * initialImpulseForce) + Vector3.up * upwardForce;
        rb.AddForce(impulse, ForceMode.Impulse);
         this.player = player;
        // Start delayed homing
        StartCoroutine(DelayedHoming(player.transform, 0.1f));
    }
}

private IEnumerator DelayedHoming(Transform playerTarget, float delay)
{
    yield return new WaitForSeconds(delay);
    StartHoming(playerTarget);
}


    void StartHoming(Transform playerTarget)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.DOMove(playerTarget.position, homingDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(Disappear);
    }

    void Disappear()
    {
        player.AddEXP(Random.Range(minExp,maxExp));
        player = null;
        ObjectPool.Instance.ReturnObject(gameObject);
    }
}
