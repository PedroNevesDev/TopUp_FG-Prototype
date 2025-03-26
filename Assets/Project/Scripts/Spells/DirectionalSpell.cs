using System;
using UnityEngine;

public class DirectionalSpell : Spell
{
    public DirectionalSpell AlterDirection( Vector3 newDir)
    {
        transform.forward = newDir; 
        return this;
    }
}
