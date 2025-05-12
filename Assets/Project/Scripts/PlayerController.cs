using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : PlayerDamageable
{
    public float speed;
    public Rigidbody myRigidbody;
    public Transform cameraTransform; // Reference to the camera

    private Vector2 move;
    public List<SpellBindings> binds = new List<SpellBindings>();
    private Dictionary<Key, Spell> bindedSpells = new Dictionary<Key, Spell>();

    public void OnMove(InputAction.CallbackContext context) => move = context.action.ReadValue<Vector2>();

    [Header("Movement Settings")]
public float rotationSmoothness = 5f;

    public Transform cardHolder;

    [Header("Awareness")]
    public float enemyAwarenessRadius = 3f;
    public LayerMask whatIsEnemy;

    public Animator myAnimator;

    public Weapon currentWeapon;

    [Range(0,1)]public float moveSpeedAttackMultiplier;

    public GameObject mesh;

    [Header("PlayerLevel")]
    private int level=1;
    public float expPerLevel=46.8f;
    public float increaseOfExpPerLevelMultiplier=0.1f;
    private float currentExp=0;

    float maxHp=70;
    void Start()
    {
        uiManager = UIManager.Instance;
        uiManager.UpdateExp(currentExp, CalculateExpNeeded());
    }
    float CalculateExpNeeded()
    {
        return expPerLevel * level + (expPerLevel*increaseOfExpPerLevelMultiplier)*level;
    }
void CheckForEnemies()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, enemyAwarenessRadius, whatIsEnemy);
    foreach (Collider hit in hits)
    {
        if (hit.TryGetComponent(out Enemy enemy) && !enemy.isActivated)
        {
            enemy.target = transform; // Make sure enemy knows who to chase
            enemy.ActivateEnemy();
        }
    }
}


public void AddEXP(float expAmount)
{
    currentExp += expAmount;
    
    // Check if we need to level up
    while (currentExp >= CalculateExpNeeded())
    {
        // Calculate extra experience after leveling up
        float extraExp = currentExp - CalculateExpNeeded();
        
        // Increase level
        level++;
        
        // Update level UI
        uiManager.UpdateLevel(level);
        
        // Reset current experience to 0 and carry over the extra experience to the next level
        currentExp = 0f;
        
        // Recursively add the extra experience to the next level
        AddEXP(extraExp);
        return;
    }

    // Update experience UI for the current level
    uiManager.UpdateExp(currentExp, CalculateExpNeeded());
}



void Movement()
{
    if (cameraTransform == null) return;

    // Get camera-relative forward/right
    Vector3 forward = cameraTransform.forward;
    forward.y = 0f;
    forward.Normalize();

    Vector3 right = cameraTransform.right;
    right.y = 0f;
    right.Normalize();

    // Get world-space move direction
    Vector3 moveDirection = (forward * move.y + right * move.x).normalized;

    // Smooth velocity change
    Vector3 targetVelocity = moveDirection * speed * (currentWeapon==true&&currentWeapon.IsAttacking()?moveSpeedAttackMultiplier:1);
    Vector3 currentVelocity = myRigidbody.linearVelocity;
    targetVelocity.y = currentVelocity.y; // preserve Y velocity

    float smoothing = 10f;
    Vector3 smoothedVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * smoothing);
Vector3 velocityChange = smoothedVelocity - myRigidbody.linearVelocity;
velocityChange.y = 0; // don't mess with vertical forces like gravity
myRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);


    // Calculate horizontal speed (ignore vertical)
    float horizontalSpeed = new Vector3(smoothedVelocity.x, 0, smoothedVelocity.z).magnitude;

    // Update animator parameters
    myAnimator.SetFloat("MovementSpeed", horizontalSpeed * 0.2f);
    myAnimator.SetBool("IsMoving", horizontalSpeed > 0.1f);

    // Rotate mesh if moving
    if (horizontalSpeed > 0.1f)
    {
        Vector3 direction = new Vector3(smoothedVelocity.x, 0, smoothedVelocity.z);
mesh.transform.forward = Vector3.Slerp(mesh.transform.forward, direction.normalized, Time.deltaTime * rotationSmoothness);

    }
}
void FixedUpdate()
{
    if (cameraTransform == null) return;

    Vector3 forward = cameraTransform.forward;
    forward.y = 0f;
    forward.Normalize();

    Vector3 right = cameraTransform.right;
    right.y = 0f;
    right.Normalize();

    Vector3 moveDirection = (forward * move.y + right * move.x).normalized;

    float appliedSpeed = speed * (currentWeapon != null && currentWeapon.IsAttacking() ? moveSpeedAttackMultiplier : 1f);
    Vector3 desiredVelocity = moveDirection * appliedSpeed;
    desiredVelocity.y = myRigidbody.linearVelocity.y;

    Vector3 velocityChange = desiredVelocity - myRigidbody.linearVelocity;
    velocityChange.y = 0f; // do not modify gravity

    myRigidbody.AddForce(velocityChange, ForceMode.VelocityChange); // smooth "pulling" feel

    // Rotation
    if (moveDirection.magnitude > 0.1f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        mesh.transform.rotation = Quaternion.Slerp(mesh.transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }

    // Animator smoothing
    float horizontalSpeed = new Vector3(myRigidbody.linearVelocity.x, 0, myRigidbody.linearVelocity.z).magnitude;
    float animationSpeed = Mathf.Lerp(myAnimator.GetFloat("MovementSpeed"), horizontalSpeed * 0.2f, Time.deltaTime * 10f);
    myAnimator.SetFloat("MovementSpeed", animationSpeed);
    myAnimator.SetBool("IsMoving", horizontalSpeed > 0.1f);
}


    void Update()
    {
        CheckForEnemies();
        if (Keyboard.current == null) return; // Prevents null reference errors

        foreach (var key in bindedSpells.Keys) // Only check bound keys
        {
            if (Keyboard.current[key] != null && (Keyboard.current[key].wasPressedThisFrame || Keyboard.current[key].isPressed))
            {
                if (bindedSpells.TryGetValue(key, out Spell newSpell))
                {
                    if(TryPrepareSpellData(newSpell.spell,out SpellEventData eventData))
                    newSpell.Use(eventData);
                }


            }
        }
        // Bind new spells if they appear (up to 10 keys)
        var spells = cardHolder.GetComponentsInChildren<Spell>();
        for (int i = 0; i < spells.Length && i < 10; i++)
        {
            Key key = (Key)System.Enum.Parse(typeof(Key), "Digit" + (i + 1));
            if (!bindedSpells.ContainsKey(key))
            {
                bindedSpells[key] = spells[i];
            }
        }
        if(Input.GetKeyDown(KeyCode.Mouse0)&& currentWeapon!=null)
        {
            currentWeapon.Attack(myAnimator);
        }
    }

bool TryPrepareSpellData(SpellSO spell, out SpellEventData spellEventData)
{
    spellEventData = null;

    switch (spell.abilityType)
    {
        case AbilityType.Cast:
            if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Vector3 basePosition = transform.position;
                Vector3 offset = transform.forward * spell.spawnOffset.z
                            + transform.right * spell.spawnOffset.x
                            + transform.up * spell.spawnOffset.y;

                Vector3 spawnPoint = basePosition + offset;
                Vector3 direction = (hit.point - spawnPoint).normalized;

                spellEventData = new SpellDirectionEventData(spawnPoint, direction);

                return true;
            }
            break;

        case AbilityType.AfflictUser:
            spellEventData = new SpellAflictEventData(this);
            return true;
    }

    return false;
}
}


[System.Serializable]
public class SpellBindings
{
    public Key keyCode;
    public Spell spellPrefab;
}