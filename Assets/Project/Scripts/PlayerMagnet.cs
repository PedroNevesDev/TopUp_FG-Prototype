using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    public float attractionRadius = 5f;
    public LayerMask attractableLayer;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attractionRadius, attractableLayer);
        foreach (var hit in hits)
        {
            IAttractable attractable = hit.GetComponent<IAttractable>();
            if (attractable != null && attractable.CanBeAttracted())
            {
                attractable.AttractTo(transform);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
