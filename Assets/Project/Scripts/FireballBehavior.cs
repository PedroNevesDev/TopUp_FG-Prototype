using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballBehavior : DirectionalSpell
{
    public int bounceCount = 3; // Number of bounces before the object is destroyed
    public float bounceLossMultiplier = 0.8f; // Multiplier for the loss of velocity after each bounce
    public float throwForce = 15f; // Force with which to throw the object initially
    public float bounceAngle = 45f; // Angle between 0 and 90 degrees to control the bounce angle
    private Rigidbody rb;

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
        // If bounce count is greater than 0, handle bounce
        if (bounceCount > 0)
        {
            // Get the normal vector of the surface the object collided with
            Vector3 contactNormal = collision.contacts[0].normal;

            // Reflect the velocity based on the normal of the surface
            Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, contactNormal);

            // Apply bounce angle control: Modify the vertical (normal) component to achieve the desired bounce angle
            reflectedVelocity = ApplyBounceAngle(reflectedVelocity, contactNormal);

            // Apply the bounce loss multiplier to reduce velocity after each bounce
            reflectedVelocity *= bounceLossMultiplier;

            // Apply the reflected velocity to the rigidbody
            rb.linearVelocity = reflectedVelocity;

            // Decrease the bounce count after each bounce
            bounceCount--;

        }
        else
        {
            // Destroy the object once bounce count reaches 0
            Destroy(gameObject);
        }
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