using UnityEngine;
using Unity.Cinemachine;

public class ZoomControl : MonoBehaviour
{
    public CinemachineFollow virtualCamera;
    public Transform cameraTransform; // Reference to the main camera
    public float zoomSpeed = 1f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    private float currentZoom = 5f;
    private CinemachineFollow transposer;
    private Vector3 initialOffset;

    public float rotationSpeed = 3f;
    private Vector3 currentRotation;

    void Start()
    {
        if (virtualCamera != null)
        {
            transposer = GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                initialOffset = transposer.FollowOffset;
            }
        }
    }

    void Update()
    {
        HandleZoom();
        HandleRotation();
    }

    void HandleZoom()
    {
        if (transposer != null)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0f)
            {
                currentZoom -= scrollInput * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                transposer.FollowOffset = initialOffset.normalized * currentZoom;
            }
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // Left-click drag
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentRotation.y += mouseX;
            currentRotation.x = Mathf.Clamp(currentRotation.x - mouseY, -80f, 80f); // Prevent flipping

            cameraTransform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        }
    }
}
