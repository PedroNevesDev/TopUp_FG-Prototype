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
    public float collectibleDelay = 0.4f; // Delay before orb can be collected

    private bool isHoming = false;
    private float spawnTime;

    PlayerController player;

private void OnEnable()
{
    spawnTime = Time.time;
    isHoming = false;
    player = null;

    // Reset any tweens
    transform.DOKill();

    // Restart particles
    if(TryGetComponent(out ParticleSystem ps))
    {
        ps.Clear();
        ps.Play();
    }
}

    private void OnTriggerEnter(Collider other)
    {
        if (isHoming) return;

        // Prevent early collection
        if (Time.time - spawnTime < collectibleDelay) return;

        if (other.TryGetComponent(out PlayerController player))
        {
            isHoming = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0f;
            Vector3 impulse = (randomDir.normalized * initialImpulseForce) + Vector3.up * upwardForce;
            rb.AddForce(impulse, ForceMode.Impulse);

            this.player = player;
            StartCoroutine(DelayedHoming(player.transform, 0.1f));
        }

    }
        private void OnTriggerStay(Collider other)
    {
        if (isHoming) return;

        // Prevent early collection
        if (Time.time - spawnTime < collectibleDelay) return;

        if (other.TryGetComponent(out PlayerController player))
        {
            isHoming = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0f;
            Vector3 impulse = (randomDir.normalized * initialImpulseForce) + Vector3.up * upwardForce;
            rb.AddForce(impulse, ForceMode.Impulse);

            this.player = player;
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
        player.AddEXP(Random.Range(minExp, maxExp));
        player = null;
        ObjectPool.Instance.ReturnObject(gameObject);
    }
}
