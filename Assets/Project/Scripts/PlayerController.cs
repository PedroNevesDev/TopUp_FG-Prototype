using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    public float speed;
    public Rigidbody rb;
    public Transform cameraTransform; // Reference to the camera

    private Vector2 move;
    public List<SpellBindings> binds = new List<SpellBindings>();
    private Dictionary<Key, Spell> bindedSpells = new Dictionary<Key, Spell>();

    public void OnMove(InputAction.CallbackContext context) => move = context.action.ReadValue<Vector2>();


    public Transform cardHolder;

    [Header("Awareness")]
    public float enemyAwarenessRadius = 3f;
    public LayerMask whatIsEnemy;

    public Animator myAnimator;

    public Weapon currentWeapon;

    [Range(0,1)]public float moveSpeedAttackMultiplier;

public GameObject mesh;

void CheckForEnemies()
{
    RaycastHit[] hits = Physics.SphereCastAll(transform.position,enemyAwarenessRadius,Vector3.up,whatIsEnemy);
    for(int i =0 ; i<hits.Length ; i++)
    {
        if(hits[i].collider.TryGetComponent(out Enemy enemy))
        {
            if(!enemy.isActivated)
            {
                enemy.ActivateEnemy();
            }
        }
    }
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
    Vector3 currentVelocity = rb.linearVelocity;
    targetVelocity.y = currentVelocity.y; // preserve Y velocity

    float smoothing = 10f;
    Vector3 smoothedVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * smoothing);
    rb.linearVelocity = smoothedVelocity;

    // Calculate horizontal speed (ignore vertical)
    float horizontalSpeed = new Vector3(smoothedVelocity.x, 0, smoothedVelocity.z).magnitude;

    // Update animator parameters
    myAnimator.SetFloat("MovementSpeed", horizontalSpeed * 0.2f);
    myAnimator.SetBool("IsMoving", horizontalSpeed > 0.1f);

    // Rotate mesh if moving
    if (horizontalSpeed > 0.1f)
    {
        Vector3 direction = new Vector3(smoothedVelocity.x, 0, smoothedVelocity.z);
        mesh.transform.forward = Vector3.Slerp(mesh.transform.forward, direction.normalized, Time.fixedDeltaTime * 10f);
    }
}


    void Update()
    {
        CheckForEnemies();
        Movement();
        if (Keyboard.current == null) return; // Prevents null reference errors

        foreach (var key in bindedSpells.Keys) // Only check bound keys
        {
            if (Keyboard.current[key] != null && (Keyboard.current[key].wasPressedThisFrame || Keyboard.current[key].isPressed))
            {
                if (bindedSpells.TryGetValue(key, out Spell newSpell))
                {
                    if(TryPrepareSpellData(newSpell.spell.abilityType,out SpellEventData eventData))
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

bool TryPrepareSpellData(AbilityType type, out SpellEventData spellEventData)
{
    spellEventData = null;

    switch (type)
    {
        case AbilityType.Cast:
            if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Vector3 direction = (hit.point - transform.position).normalized;
                spellEventData = new SpellDirectionEventData(transform.position, direction);
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