using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    public SpellSO spell;
    public int maxBounceCount = 3; // Maximum number of bounces before returning to pool
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
        Vector3 contactNormal = collision.contacts[0].normal;

        // Reflect velocity normally
        Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, contactNormal);
        reflectedVelocity = ApplyBounceAngle(reflectedVelocity, contactNormal);

        // Ensure it never falls below a minimum threshold
        float minVelocity = 3f; // Adjust as needed
        if (reflectedVelocity.magnitude < minVelocity)
        {
            reflectedVelocity = reflectedVelocity.normalized * minVelocity;
        }

        rb.linearVelocity = reflectedVelocity * bounceLossMultiplier;
        currentBounceCount--;
        // Decrease bounce count or return to pool
        if (currentBounceCount < 0)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out Damageable component))
        {
            component.TakeDamage(spell.ProccessedValue(),component.transform.position-transform.position *0.1f);
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