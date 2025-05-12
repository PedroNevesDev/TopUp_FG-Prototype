using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Enemy : Damageable
{
    public Collider attackCollision;
    public DamageArea damageArea;
    public float attackpreptime;
    public Transform target;

    public Transform maskTarget;
    public GameObject objectToSpawn;
    public Vector3 skySpawnOffset = new Vector3(0, 20f, 0);
    public float moveDuration = 2f;
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    public bool isActivated = false;
    private bool isActivating = false;
    private bool isAttacking = false;
    private GameObject spawnedObject;

    protected override void OnEnable()
    {
        base.OnEnable();

        isActivated = false;
        isAttacking = false;
        damageArea.gameObject.SetActive(false);
        target = null;
        Destroy(spawnedObject);
    }
    private void Update()
{
    if (!isActivated || target == null) return;

    float distance = Vector3.Distance(transform.position, target.position);

    // Handle movement when within detection range but outside attack range
    if (distance < detectionRadius && distance > attackRange)
    {
        attackCollision.enabled = false;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        // Only move if not attacking
        if (!isAttacking)
        {
            transform.position += direction * (moveSpeed -slow*moveSpeed    )* Time.deltaTime;

            // Rotate toward player (only if not attacking)
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 flatRotation = new Vector3(0, lookRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(flatRotation), rotationSpeed * Time.deltaTime);
        }
    }
    else if (distance <= attackRange)
    {
        // Check if enemy is almost facing the player (within 45 degrees, or adjust to your liking)
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        float angleToTarget = Vector3.Angle(transform.forward, direction);

        if (!isAttacking && angleToTarget < 45f) // Enemy can attack if within 45 degrees
        {
            StartCoroutine(StartAttack());
            Debug.Log("Enemy is in range and preparing to attack.");
        }
        else if (!isAttacking)
        {
            // Rotate towards the player if not facing them properly (if too far off)
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 flatRotation = new Vector3(0, lookRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(flatRotation), rotationSpeed * Time.deltaTime);
        }
    }
}




    IEnumerator StartAttack()
    {
        isAttacking = true;

        damageArea.damage = globalStatsManager.enemyDamage;
        damageArea.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackpreptime);

        attackCollision.enabled = true;

        yield return new WaitForSeconds(0.1f); // Small window for damage
        attackCollision.enabled = false;
        damageArea.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f); // Attack cooldown
        isAttacking = false;
    }

    public void ActivateEnemy()
    {
        if (target == null || isActivated || isActivating) return;

        isActivating = true;

        Vector3 spawnPosition = target.position + skySpawnOffset;
        spawnedObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

        Vector3 midPoint = (spawnPosition + maskTarget.position) / 2 + Random.insideUnitSphere * 3f;
        midPoint.y = Mathf.Max(midPoint.y, maskTarget.position.y + 1f);

        Vector3[] path = new Vector3[]
        {
            spawnPosition,
            midPoint,
            maskTarget.position
        };

        spawnedObject.transform.DOPath(path, moveDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                Debug.Log("Mask landed. Enemy activated.");
                spawnedObject.transform.SetParent(transform.GetChild(0));
                spawnedObject.transform.position = maskTarget.transform.position;
                spawnedObject.transform.forward = maskTarget.forward;

                isActivated = true;
                isActivating = false;

                if (spawnedObject.TryGetComponent<Rigidbody>(out var rb))
                    rb.isKinematic = true;
                
                controlableResist = 1;
            });
    }
}
