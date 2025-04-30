using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Spell : MonoBehaviour
{
    public Card myCard;
    public SpellSO spell;

    public GlobalStatsManager globalStats;
    void Start()
    {
        myCard.Setup(spell);
        globalStats = GlobalStatsManager.Instance;
    }

    public void Use(SpellEventData data)
    {
        switch (data)
        {
            case SpellDirectionEventData directionData:
                Cast(directionData.position, directionData.direction);
                break;

            case SpellAflictEventData afflictData:
                AfflictUser(afflictData.afflictedTarget); // or AfflictTarget(afflictData.afflictedTarget);
                break;

            default:
                Debug.LogWarning("Unknown spell event data.");
                break;
        }
    }
    protected virtual void Cast(Vector3 position, Vector3 direction)
    {
        
    }

    protected virtual void AfflictUser(PlayerController playerController)
    {
        PlayerController player = playerController;

        //Affect Player
    }
}

public abstract class SpellEventData{}

public class SpellAflictEventData : SpellEventData
{
    public PlayerController afflictedTarget;

    public SpellAflictEventData(PlayerController target)
    {
        afflictedTarget = target;
    }
}

public class SpellDirectionEventData : SpellEventData
{
    public Vector3 position;
    public Vector3 direction;

    public SpellDirectionEventData(Vector3 pos, Vector3 dir)
    {
        position = pos;
        direction = dir;
    }
}
