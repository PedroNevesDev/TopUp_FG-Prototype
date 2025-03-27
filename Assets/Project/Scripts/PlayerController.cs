using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;

    private Vector2 move;
    public List<SpellBindings> binds = new List<SpellBindings>();
    private Dictionary<Key, Spell> bindedSpells = new Dictionary<Key, Spell>();

    public void OnMove(InputAction.CallbackContext context) => move = context.action.ReadValue<Vector2>();

    void Movement()
    {
        Vector3 newVel = new Vector3(move.x, rb.linearVelocity.y, move.y) * speed;
        newVel.y = rb.linearVelocity.y;
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
                // Get the spell bound to this key
                if (bindedSpells.TryGetValue(key, out Spell newSpell))
                {
                    Debug.Log("Casting Spell: " + newSpell.name);

                    // Cast the spell when the key is held or pressed
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                    {
                        Vector3 direction = (hit.point - transform.position).normalized; // Correct direction

                        // Let the spell handle its cooldown and casting logic
                        newSpell.Cast(transform.position, direction);
                    }
                }

                break; // Stop after detecting the first key
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