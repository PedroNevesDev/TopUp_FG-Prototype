using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballBehavior : DirectionalSpell
{
    public int bounceCount = 3; // Number of bounces before the object is destroyed
    public float bounceLossMultiplier = 0.8f; // Multiplier for the loss of velocity after each bounce
    public float throwForce = 15f; // Force with which to throw the object initially
    public float bounceAngle = 45f; // Angle between 0 and 90 degrees to control the bounce angle
    private Rigidbody rb;

    public float additionalGravity = 20f;
    public GameObject collisionEffect; // Assign your particle prefab in the Inspector

    List<GameObject> objcs = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure Rigidbody is set up properly
        rb.useGravity = true; // Make sure gravity is enabled
        rb.linearDamping = 0f; // Ensure no linear drag (so it doesn't slow down unnaturally)

        // Apply a random initial throw on the X and Z axes
        ThrowObject();
    }

    private void ThrowObject()
    {
        // Generate a random direction on the X and Z axes (ignoring Y axis)
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);

        // Normalize the direction to ensure a consistent magnitude
        Vector3 randomDirection = transform.forward;

        // Apply force to the rigidbody in the random direction
        rb.AddForce(randomDirection * throwForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionEffect != null)
        {
            Vector3 spawnPosition = collision.contacts[0].point + collision.contacts[0].normal * 0.1f; // Offset VFX slightly above surface
            objcs.Add(Instantiate(collisionEffect, spawnPosition, Quaternion.LookRotation(collision.contacts[0].normal)));
        }

        if (bounceCount > 0)
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, contactNormal);
            reflectedVelocity = ApplyBounceAngle(reflectedVelocity, contactNormal);
            reflectedVelocity *= bounceLossMultiplier;
            rb.linearVelocity = reflectedVelocity;
            bounceCount--;
        }
        else
        {
            objcs.ForEach(obj=>Destroy(obj,1));
            Destroy(gameObject);
        }
    }
    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * additionalGravity, ForceMode.Acceleration);
    }

    // Apply the bounce angle control
    private Vector3 ApplyBounceAngle(Vector3 reflectedVelocity, Vector3 contactNormal)
    {
        // Calculate the desired bounce angle in radians
        float desiredAngle = Mathf.Clamp(bounceAngle, 0f, 90f); // Ensure it's between 0 and 90 degrees
        float desiredAngleRad = Mathf.Deg2Rad * desiredAngle;

        // Project the reflected velocity onto the plane perpendicular to the contact normal
        Vector3 tangent = Vector3.ProjectOnPlane(reflectedVelocity, contactNormal).normalized;

        // Calculate the vertical component (along the normal) that will maintain the desired bounce angle
        float verticalSpeed = reflectedVelocity.magnitude * Mathf.Sin(desiredAngleRad);

        // Apply the angle adjustment by scaling the vertical component along the normal direction
        Vector3 verticalVelocity = contactNormal * verticalSpeed;

        // Combine the adjusted vertical velocity with the horizontal tangent velocity
        Vector3 adjustedVelocity = tangent * reflectedVelocity.magnitude;

        // Return the final adjusted velocity
        return adjustedVelocity + verticalVelocity;
    }
}