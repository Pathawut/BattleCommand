using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CommandType { Move, Attack, Defend }

[System.Serializable]
public class Command
{
    public CommandType type;
    public Vector3 targetPosition;
    public EnemyUnit targetUnit;        // For attack command
    public float delay;
    public bool isPending;

    public Command(CommandType t, Vector3 pos, float d, EnemyUnit target = null)
    {
        type = t;
        targetPosition = pos;
        targetUnit = target;
        delay = d;
        isPending = true;
    }
}

public class CommandSystem : MonoBehaviour
{
    public static CommandSystem Instance { get; private set; }

    [Header("Communication Delay")]
    [Range(0.5f, 3f)]
    public float commandDelay = 1.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>Issue a command to a unit. Returns the Command object for UI display.</summary>
    public Command IssueCommand(Unit unit, CommandType type, Vector3 targetPos, EnemyUnit targetUnit = null)
    {
        if (unit == null || unit.IsDead) return null;

        Command cmd = new Command(type, targetPos, commandDelay, targetUnit);
        unit.EnqueueCommand(cmd);
        return cmd;
    }
}
