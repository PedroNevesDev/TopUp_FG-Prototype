using UnityEngine;

public class ParticleReturn : MonoBehaviour
{
    [SerializeField] GameObject targetGameObject;
    private void OnParticleSystemStopped()
    {
        ObjectPool.Instance.ReturnObject(targetGameObject);
    }
}