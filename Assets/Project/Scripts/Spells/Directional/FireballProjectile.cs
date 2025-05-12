using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    public SpellSO spell;
    public float bounceLossMultiplier = 0.8f; // Loss of velocity per bounce
    public float throwForce = 15f; // Initial throw force
    public float additionalGravity = 20f; // Additional gravity applied to the fireball
    public float bounceAngle = 45f; // Bounce angle (between 0 and 90 degrees)
    public GameObject collisionEffect; // Particle effect for collisions

    public Rigidbody rb;
    private int currentBounceCount;

    public List<TrailRenderer> trails = new List<TrailRenderer>();

    public void OnEnable()
    {
        rb.linearVelocity = Vector3.zero; // Ensure no old forces remain
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp(); // Reactivate physics engine processing

        currentBounceCount = spell.GetBounces();

        // Throw the object after ensuring it's fully active
        ThrowObject();

        trails.ForEach(t=> t.emitting = true);
    }



    private void ThrowObject()
    {
        // Use transform.forward to determine the throw direction, and apply the throw force
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject == gameObject) return;

    Vector3 contactNormal = collision.contacts[0].normal;

    // Reflect velocity
    Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, contactNormal);
    reflectedVelocity = ApplyBounceAngle(reflectedVelocity, contactNormal);

    // Ensure minimum velocity
    float minVelocity = 3f;
    if (reflectedVelocity.magnitude < minVelocity)
    {
        reflectedVelocity = reflectedVelocity.normalized * minVelocity;
    }

    rb.linearVelocity = reflectedVelocity * bounceLossMultiplier;

    // Only reduce bounce count for upward-ish hits
    if (Vector3.Dot(contactNormal, Vector3.up) > 0.5f)
    {
        currentBounceCount--;
    }

    // Pooled collision effect, aligned up with the normal
    if (collisionEffect != null)
    {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, contactNormal); // Forward stays fixed, up becomes normal
        ObjectPool.Instance.GetObject(collisionEffect, collision.contacts[0].point, rotation);
    }

    // Return to pool if out of bounces
    if (currentBounceCount < 0)
    {
        ReturnToPool();
    }
}



    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")&&other.TryGetComponent(out Damageable component))
        {
            component.TakeDamage(spell.ProccessedValue(),component.transform.position-transform.position *0.1f,0.1f);
            ReturnToPool();
            return;
        }
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector3.zero;  // Reset movement
        rb.angularVelocity = Vector3.zero;  // Reset rotation force
        rb.Sleep(); // Ensure it's fully stopped
        trails.ForEach(t =>{
            t.emitting = false;
            t.Clear();

        } );

        // Return to pool
        ObjectPool.Instance.ReturnObject(gameObject);
    }

    void FixedUpdate()
    {
        // Apply additional gravity
        rb.AddForce(Vector3.down * additionalGravity, ForceMode.Acceleration);

        // Debug if the fireball is stuck (not moving but still active)
        if (rb.linearVelocity.magnitude < 0.1f && currentBounceCount > 0)
        {
            ThrowObject(); // Try re-throwing it
        }
    }

    // Apply the bounce angle to the reflected velocity
    private Vector3 ApplyBounceAngle(Vector3 reflectedVelocity, Vector3 contactNormal)
    {
        // Calculate the desired bounce angle in radians
        float desiredAngle = Mathf.Clamp(bounceAngle, 0f, 90f);
        float desiredAngleRad = Mathf.Deg2Rad * desiredAngle;

        // Project the reflected velocity onto the plane perpendicular to the contact normal
        Vector3 tangent = Vector3.ProjectOnPlane(reflectedVelocity, contactNormal).normalized;

        // Calculate the vertical component of the bounce
        float verticalSpeed = reflectedVelocity.magnitude * Mathf.Sin(desiredAngleRad);

        // Apply the vertical component along the contact normal
        Vector3 verticalVelocity = contactNormal * verticalSpeed;

        // Return the final adjusted velocity by combining the horizontal (tangent) and vertical components
        return tangent * reflectedVelocity.magnitude + verticalVelocity;
    }
}