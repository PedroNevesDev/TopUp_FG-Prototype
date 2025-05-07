using UnityEngine;

public class CallGenerator : MonoBehaviour
{
    public GameObject generator;
    public GameObject deactive;
    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            deactive.SetActive(false);
            generator.SetActive(true);
        }

    }
}
