using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6)
        {
            ObjectPool.Instance.ReturnObject(collision.gameObject);
        }
    }
}
