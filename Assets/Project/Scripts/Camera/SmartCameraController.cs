using UnityEngine;
using Unity.Cinemachine;

public class SmartCameraController : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public Transform followTarget;
    public LayerMask collisionLayers;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    private float targetZoom = 5f;

    [Header("Orbit Settings")]
    public float rotationSpeed = 3f;
    private float yaw = 0f;
    private float pitch = 20f;

    [Header("Smoothing")]
    public float followSmoothTime = 0.08f;
    private Vector3 currentFollowOffset;
    private Vector3 followVelocity = Vector3.zero;

    private CinemachineFollow follow;

    void Start()
    {
        if (virtualCamera == null || followTarget == null)
        {
            Debug.LogError("Missing VirtualCamera or Follow Target");
            enabled = false;
            return;
        }

        follow = virtualCamera.GetComponent<CinemachineFollow>();
        if (follow == null)
        {
            Debug.LogError("CinemachineFollow module not found on virtual camera.");
            enabled = false;
            return;
        }

        currentFollowOffset = follow.FollowOffset;
        targetZoom = Mathf.Clamp(currentFollowOffset.magnitude, minZoom, maxZoom);

        Vector3 angles = virtualCamera.transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        HandleZoom();
        HandleRotation();
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredOffset = rotation * new Vector3(0f, 0f, -targetZoom);
        Vector3 origin = followTarget.position;
        Vector3 desiredCameraPos = origin + desiredOffset;
        Vector3 castDirection = (desiredCameraPos - origin).normalized;

        if (Physics.SphereCast(origin, 0.3f, castDirection, out RaycastHit hit, targetZoom, collisionLayers))
        {
            float adjustedDistance = Mathf.Clamp(hit.distance, minZoom, targetZoom);
            desiredOffset = castDirection * adjustedDistance;
        }

        currentFollowOffset = Vector3.SmoothDamp(currentFollowOffset, desiredOffset, ref followVelocity, followSmoothTime);
        follow.FollowOffset = currentFollowOffset;

        virtualCamera.transform.rotation = rotation;
    }

    void OnDrawGizmos()
    {
        if (followTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 desiredOffset = Quaternion.Euler(pitch, yaw, 0f) * new Vector3(0f, 0f, -targetZoom);
            Vector3 origin = followTarget.position;
            Gizmos.DrawLine(origin, origin + desiredOffset);
            Gizmos.DrawWireSphere(origin + desiredOffset, 0.3f);
        }
    }
}