using UnityEngine;
using DG.Tweening;

public class Enemy : Damageable
{
    public Transform target; // The player

    public Transform maskTarget;
    public GameObject objectToSpawn; // The mask
    public Vector3 skySpawnOffset = new Vector3(0, 20f, 0);
    public float moveDuration = 2f;
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    public bool isActivated = false;
    private GameObject spawnedObject;
    private bool isActivating = false;


    private void Update()
    {
        if (!isActivated || target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < detectionRadius && distance > attackRange)
        {
            // Move toward player
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f; // Prevent upward movement
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face player on Y axis only
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 flatRotation = new Vector3(0, lookRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(flatRotation), rotationSpeed * Time.deltaTime);
        }
        else if (distance <= attackRange)
        {
            Debug.Log("Enemy is in range and preparing to attack.");
            // Trigger attack animation or logic
        }
    }

public void ActivateEnemy()
{
    if (target == null || isActivated || isActivating) return;

    isActivating = true;

    // Spawn visual mask above target
    Vector3 spawnPosition = target.position + skySpawnOffset;
    spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

    // Create curved path
    Vector3 midPoint = (spawnPosition + maskTarget.position) / 2 + Random.insideUnitSphere * 3f;
    midPoint.y = Mathf.Max(midPoint.y, maskTarget.position.y + 1f); // ensure arc stays above target

    Vector3[] path = new Vector3[]
    {
        spawnPosition,
        midPoint,
        maskTarget.position // correct target point
    };

    // Animate mask descending to target
    spawnedObject.transform.DOPath(path, moveDuration, PathType.CatmullRom)
        .SetEase(Ease.InOutSine)
        .OnComplete(() =>
        {
            Debug.Log("Mask landed. Enemy activated.");

            spawnedObject.transform.SetParent(transform.GetChild(0));
            transform.forward = maskTarget.forward;


            isActivated = true;
            isActivating = false;

            // Optional cleanup: disable physics
            if (spawnedObject.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;
        });
}

}
