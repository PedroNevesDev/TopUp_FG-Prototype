using UnityEngine;
using DG.Tweening;

public class Enemy : Damageable
{
    public Transform target;
    public GameObject objectToSpawn;
    public Vector3 skySpawnOffset = new Vector3(0, 20f, 0);
    public float moveDuration = 2f;

 
    public bool isActivated = false;
    public void ActivateEnemy()
    {
        isActivated = true; 
        // 1. Spawn in the sky above the target (or use a fixed sky position)
        Vector3 spawnPosition = target.position + skySpawnOffset;
        GameObject obj = Instantiate(objectToSpawn, spawnPosition, target.rotation,transform);

        // 2. Generate random curve points for the path
        Vector3 midPoint = (spawnPosition + target.position) / 2;
        midPoint += Random.insideUnitSphere * 3f; // Add random curve

        Vector3[] path = new Vector3[] {
            spawnPosition,
            midPoint,
            target.position
        };

        // 3. Animate using DOTween's path system
        obj.transform.DOPath(path, moveDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                Debug.Log("Arrived!");
                obj.transform.position = target.position;
                obj.transform.rotation = target.rotation;
                // Optional: trigger VFX or destroy
            });
    }
}