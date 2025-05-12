using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XPOrb : MonoBehaviour, IAttractable
{
    public int minEXP = 1;
    public int maxEXP = 5;
    public float attractionSpeed = 10f;
    private bool isBeingAttracted = false;
    private Transform target;

    private ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        isBeingAttracted = false;
        target = null;

        // Reset and play the particle system
        if (particleSystem != null)
        {
            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    void Update()
    {
        if (!isBeingAttracted || target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, attractionSpeed * Time.deltaTime);
    }

    public void AttractTo(Transform playerTransform)
    {
        isBeingAttracted = true;
        target = playerTransform;
    }

    public bool CanBeAttracted()
    {
        return true; // Optional logic (e.g., cooldown, only if visible, etc.)
    }

    void OnTriggerEnter(Collider other)
    {

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            int xp = Random.Range(minEXP, maxEXP + 1);
            player.AddEXP(xp);
            ObjectPool.Instance?.ReturnObject(gameObject); // Safe pooling fallback
        }

        // Use pooling if you have it, otherwise destroy

        // Destroy(gameObject); // fallback if no pooling
    }
}