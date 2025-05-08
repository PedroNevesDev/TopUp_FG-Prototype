using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public List<CinemachineCamera> virtualCameras = new List<CinemachineCamera>();

    public CinemachineImpulseSource myImpulseSource;
    private int currentCamera = 0;
    
    void Start()
    {
        ToggleCameras(currentCamera);
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            currentCamera = (currentCamera + 1) % virtualCameras.Count;
            ToggleCameras(currentCamera);
        }
    }

    void ToggleCameras(int index)
    {
        // Disable all cameras first
        foreach (var cam in virtualCameras)
        {
            cam.gameObject.SetActive(false);
        }
        
        // Enable the selected camera
        virtualCameras[index].gameObject.SetActive(true);
    }

    // Shakes ONLY the currently active camera with a directional force
    public void ShakeActiveCamera(float force, Vector3 direction)
    {
        if (virtualCameras.Count == 0) return;
        
        myImpulseSource.GenerateImpulseWithVelocity(direction.normalized * force);
    }
}