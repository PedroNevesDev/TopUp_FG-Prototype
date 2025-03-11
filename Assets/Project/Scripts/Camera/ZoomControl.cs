
using Unity.Cinemachine;
using UnityEngine;

public class ZoomControl : MonoBehaviour
{
    public CinemachineFollow virtualCamera;
    public float zoomSpeed = 1f; // Speed of zooming
    public float minZoom = 2f;   // Minimum zoom distance
    public float maxZoom = 10f;  // Maximum zoom distance
    private float currentZoom = 5f; // Starting zoom value

    private CinemachineFollow transposer; // Access Cinemachine Transposer for camera follow
    private Vector3 initialOffset;

    void Start()
    {
        if (virtualCamera != null)
        {
            // Get the transposer component of the virtual camera
            transposer = GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                initialOffset = transposer.FollowOffset;
            }
        }
    }

    void Update()
    {
        if (transposer != null)
        {
            // Get the scroll input
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Adjust the zoom level based on scroll input
            if (scrollInput != 0f)
            {
                // Calculate the new zoom level
                currentZoom -= scrollInput * zoomSpeed;

                // Clamp the zoom value to stay within the defined range
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                // Set the new follow offset based on the zoom level
                transposer.FollowOffset = initialOffset.normalized * currentZoom;
            }
        }
    }
}