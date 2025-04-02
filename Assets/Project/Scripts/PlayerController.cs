using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;
    public Transform cameraTransform; // Reference to the camera

    private Vector2 move;
    public List<SpellBindings> binds = new List<SpellBindings>();
    private Dictionary<Key, Spell> bindedSpells = new Dictionary<Key, Spell>();

    public void OnMove(InputAction.CallbackContext context) => move = context.action.ReadValue<Vector2>();

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

    void Start()
    {
        binds.ForEach(b => bindedSpells.Add(b.keyCode, b.spellPrefab));
    }

    void Update()
    {
        Movement();
        if (Keyboard.current == null) return; // Prevents null reference errors

        foreach (var key in bindedSpells.Keys) // Only check bound keys
        {
            if (Keyboard.current[key] != null && (Keyboard.current[key].wasPressedThisFrame || Keyboard.current[key].isPressed))
            {
                if (bindedSpells.TryGetValue(key, out Spell newSpell))
                {
                    Debug.Log("Casting Spell: " + newSpell.name);

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                    {
                        Vector3 direction = (hit.point - transform.position).normalized;
                        newSpell.Cast(transform.position, direction);
                    }
                }

                break;
            }
        }
    }
}


[System.Serializable]
public class SpellBindings
{
    public Key keyCode;
    public Spell spellPrefab;
}