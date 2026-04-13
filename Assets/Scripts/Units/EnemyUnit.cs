using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum unitType  { Basic, Heavy, Flanker }

/// <summary>
/// Enemy AI unit.
/// Movement  : NavMeshAgent (NavMeshPlus for 2D).
/// Attacking : spawns a Projectile that travels toward its target.
/// FSM       : Idle → Chase (NavMesh) → Attack (fire projectile).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyUnit : Unit
{
    // ── INSPECTOR ─────────────────────────────────────────────────

    [Header("Enemy Config")]
    public float     detectionRange= 9f;


    // ── PRIVATE ───────────────────────────────────────────────────
    private Base    playerBase;

    // ─────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        ResetStatus();

        CurrentHP = maxHP;

        // NavMeshAgent — 2D config
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation  = false;
        agent.updateUpAxis    = false;
        agent.speed           = moveSpeed;
        agent.stoppingDistance = attackRange * 0.85f;
        agent.acceleration    = 20f;
        agent.angularSpeed    = 0f;

        // Find player base
        Base[] bases = FindObjectsByType<Base>(FindObjectsSortMode.None);
        foreach (var b in bases)
            if (b.isPlayerBase) { playerBase = b; break; }
    }


    void Update()
    {
        if (IsDead || GameManager.Instance.IsGameOver) return;
        attackTimer -= Time.deltaTime;
        RunFSM();
        UpdateRotation();
        UpdateHPBar();
    }

    // ─────────────────────────────────────────────────────────────
    // FSM
    // ─────────────────────────────────────────────────────────────

    void RunFSM()
    {
        Unit nearestPlayer = FindNearestPlayerUnit(detectionRange);

        if (nearestPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, nearestPlayer.transform.position);

            if (dist <= attackRange)
            {
                StopMoving();
                StateAttackUnit(nearestPlayer);
            }
            else
            {
                State = UnitState.Moving;
                NavigateTo(nearestPlayer.transform.position);
            }
        }
        else if (playerBase != null)
        {
            float distBase = Vector3.Distance(transform.position, playerBase.Target.position);

            if (distBase <= attackRange)
            {
                StopMoving();
                StateAttackBase();
            }
            else
            {
                State = UnitState.Moving;
                NavigateTo(playerBase.Target.position);
            }
        }
        else
        {
            State = UnitState.Idle;
            StopMoving();
        }
    }

    void StateAttackUnit(Unit target)
    {
        State = UnitState.Attacking;
        if (attackTimer > 0f || target.IsDead) return;

        FireProjectile(target.transform, target.transform.position, attackDamage);
        attackTimer = attackCooldown;
    }

    void StateAttackBase()
    {
        State = UnitState.Attacking;
        if (attackTimer > 0f) return;

        if (playerBase != null)
        {
            // Fire a projectile at the base
            FireProjectile(playerBase.Target, playerBase.Target.position, attackDamage);
        }
        attackTimer = attackCooldown;
    }

    // ─────────────────────────────────────────────────────────────
    // FIRE PROJECTILE
    // ─────────────────────────────────────────────────────────────

    void FireProjectile(Transform target, Vector3 targetPos, float dmg)
    {
        if (projectilePrefab == null)
        {
            // Instant-hit fallback if no prefab assigned
            Unit u = target.GetComponent<Unit>();
            if (u != null && !(u is EnemyUnit)) u.TakeDamage(dmg);
            Base b = target.GetComponent<Base>();
            if (b != null && b.isPlayerBase) b.TakeDamage(dmg);
            return;
        }

        Vector3 spawnPos = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + Vector3.up * 0.1f;

        GameObject go   = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile proj = go.GetComponent<Projectile>();
        if (proj == null) { Destroy(go); return; }

        AudioManager.Instance.PlaySFX(SFX.Fire);

        proj.speed  = projectileSpeed;
        proj.homing = false;  // enemies fire straight
        proj.Fire(target, dmg, false); // hitsEnemy=false → damages player units/base
    }


    // ─────────────────────────────────────────────────────────────
    // DAMAGE / DEATH
    // ─────────────────────────────────────────────────────────────

    public override void Die()
    {
        IsDead = true;
        StopMoving();
        agent.enabled = false;
        StartCoroutine(DeathEffect());
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────

    Unit FindNearestPlayerUnit(float range)
    {
        Unit[] all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        Unit closest = null;
        float minD = range;

        foreach (var u in all)
        {
            if (u.IsDead || u is EnemyUnit) continue;
            float d = Vector3.Distance(transform.position, u.transform.position);
            if (d < minD) { minD = d; closest = u; }
        }
        return closest;
    }
}
