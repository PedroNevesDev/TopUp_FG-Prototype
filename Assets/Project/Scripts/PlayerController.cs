using System;
using System.Collections.Generic;
using System.Linq;
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

        // Get camera's forward and right directions, ignoring Y (so movement stays level)
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();

        // Convert input into world-space movement
        Vector3 moveDirection = (forward * move.y + right * move.x).normalized;

        Vector3 newVel = moveDirection * speed;
        newVel.y = rb.linearVelocity.y; // Preserve vertical velocity
        rb.linearVelocity = newVel;
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