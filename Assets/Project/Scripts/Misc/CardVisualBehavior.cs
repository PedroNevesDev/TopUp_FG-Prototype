using UnityEngine;

public class CardVisualBehavior : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float levitationHeight = 0.2f;
    public float levitationSpeed = 2f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate around Y-axis
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        // Levitate up and down using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * levitationSpeed) * levitationHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}