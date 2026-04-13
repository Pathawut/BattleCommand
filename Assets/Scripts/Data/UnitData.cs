using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Game/UnitData Data")]
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public UnitType unitType;
    public string unitName;

    [Header("Stats")]
    public int maxHP;
    public float moveSpeed;

    [Header("Combat")]
    public float attackRange;
    public float attackDamage;
    public float attackCooldown;

    [Header("Production")]
    public float productionTime;
}
