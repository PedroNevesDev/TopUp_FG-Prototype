using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;

    Vector2 move;
   public  List<SpellBindings> binds = new List<SpellBindings>();

    public void OnMove(InputAction.CallbackContext context)=>move = context.action.ReadValue<Vector2>();

    void Movement()
    {
        Vector3 newVel = new Vector3(move.x,rb.linearVelocity.y,move.y) * speed;

        newVel.y = rb.linearVelocity.y;
        rb.linearVelocity = newVel;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        binds.ForEach(b=> bindedSpells.Add(b.keyCode,b.spellPrefab));
    }
    public Dictionary<Key, Spell> bindedSpells = new Dictionary<Key, Spell>();

    void Update()
    {
        Movement();
        if (Keyboard.current == null) return; // Prevents null reference errors

        foreach (var key in bindedSpells.Keys) // Only check bound keys
        {
            if (Keyboard.current[key] != null && Keyboard.current[key].wasPressedThisFrame)
            {
                Debug.Log("Key Pressed: " + key);

                if (bindedSpells.TryGetValue(key, out Spell newSpell))
                {
                    Debug.Log("Casting Spell: " + newSpell.name);
                    Cast(newSpell);
                }
                break; // Stop after detecting the first key
            }
        }
    }
    void Cast(Spell spellToCast)
    {
        if(spellToCast is DirectionalSpell directionalSpell)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Vector3 direction = (hit.point - transform.position).normalized; // Correct direction
                Instantiate(directionalSpell,transform.position,Quaternion.identity).AlterDirection(direction);
                return;
            }
        }

        Instantiate(spellToCast);
    }
    
}
[System.Serializable]
public class SpellBindings
{
    public Key keyCode;
    public Spell spellPrefab;
} 
