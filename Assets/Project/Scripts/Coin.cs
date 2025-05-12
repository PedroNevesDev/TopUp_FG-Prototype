using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour, IAttractable
{
    public int minGold = 1;
    public int maxGold = 5;
    public float attractionSpeed = 10f;
    private bool isBeingAttracted = false;
    private Transform target;


    void OnEnable()
    {
        isBeingAttracted = false;
        target = null;
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
            int xp = Random.Range(minGold, maxGold + 1);
            ShopManager.Instance.AddMoney(xp);
            ObjectPool.Instance?.ReturnObject(gameObject); // Safe pooling fallback
        }

        // Use pooling if you have it, otherwise destroy

        // Destroy(gameObject); // fallback if no pooling
    }
    void OnCollisionEnter(Collision other)
        
    {

        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            int xp = Random.Range(minGold, maxGold + 1);
            ShopManager.Instance.AddMoney(xp);
            ObjectPool.Instance?.ReturnObject(gameObject); // Safe pooling fallback
        }

        // Use pooling if you have it, otherwise destroy

        // Destroy(gameObject); // fallback if no pooling
    }
}