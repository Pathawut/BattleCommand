using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum UnitType  { Heavy, Scout, Sniper }
public enum UnitState { Idle, Moving, Attacking, Defending }

/// <summary>
/// Player-controlled unit.
/// Movement  : NavMeshAgent (NavMeshPlus package for 2D — see SETUP_GUIDE).
/// Attacking : spawns a Projectile prefab that physically travels to the target.
/// Commands  : queued with Communication Delay before executing (core GDD mechanic).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    // ── INSPECTOR ─────────────────────────────────────────────────
    [Header("Unit Data")]
    public UnitData unitData;

    [Header("Unit Config")]
    public UnitType unitType;
    public string   unitName      = "Unit";
    public int      maxHP         = 100;
    public float    moveSpeed     = 3f;
    public float    attackRange   = 4f;
    public float    attackDamage  = 20f;
    public float    attackCooldown= 1.2f;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;      // Assign Projectile prefab in Inspector
    public Transform  muzzlePoint;           // Optional empty child as barrel tip
    public float      projectileSpeed = 8f;
    public bool       homingShot      = false;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color idleColor    = Color.white;

    [Tooltip("Degrees per second the unit rotates to face its movement direction. 0 = instant snap.")]
    public float rotationSpeed = 360f;

    [Header("UI")]
    public Slider     hpSlider;
    public GameObject delayIndicator; // shown during communication lag

    // ── STATE ─────────────────────────────────────────────────────
    public int       CurrentHP  { get; set; }
    public bool      IsDead     { get; set; }
    public UnitState State      { get; set; } = UnitState.Idle;
    public bool      IsSelected { get; set; }

    protected NavMeshAgent agent;
    protected float attackTimer;

    // ── PRIVATE ───────────────────────────────────────────────────
    private Queue<Command> commandQueue = new Queue<Command>();
    private Command        currentCommand;
    
    private bool           isExecuting;
    private float          fuelDrainTimer;
    private LineRenderer   waypointLine;

    // ─────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        ResetStatus();

        CurrentHP = maxHP;

        // NavMeshAgent — 2D config (requires NavMeshPlus)
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation  = false;
        agent.updateUpAxis    = false;
        agent.speed           = moveSpeed;
        agent.stoppingDistance = 0.15f;
        agent.acceleration    = 25f;
        agent.angularSpeed    = 0f;

        // Waypoint dashed line
        waypointLine = gameObject.AddComponent<LineRenderer>();
        waypointLine.startWidth    = 0.06f;
        waypointLine.endWidth      = 0.02f;
        waypointLine.material      = new Material(Shader.Find("Sprites/Default"));
        waypointLine.startColor    = new Color(1f, 1f, 0f, 0.6f);
        waypointLine.endColor      = new Color(1f, 1f, 0f, 0.15f);
        waypointLine.textureMode   = LineTextureMode.Tile;
        waypointLine.positionCount = 0;
        waypointLine.sortingOrder  = 5;

        if (delayIndicator) delayIndicator.SetActive(false);

       
    }

    protected void ResetStatus()
    {
        if(unitData != null)
        {
            unitName = unitData.unitName;
            maxHP = unitData.maxHP;
            moveSpeed = unitData.moveSpeed;
            attackRange = unitData.attackRange;
            attackDamage = unitData.attackDamage;
            attackCooldown = unitData.attackCooldown;
        }
    }

    void Update()
    {
        if (IsDead || GameManager.Instance.IsGameOver) return;

        attackTimer    -= Time.deltaTime;
        fuelDrainTimer -= Time.deltaTime;

        // Command queue processing
        if (!isExecuting && commandQueue.Count > 0)
            StartCoroutine(ExecuteCommand(commandQueue.Dequeue()));
        else if (!isExecuting)
            IdleBehavior();

        UpdateVisuals();
        UpdateRotation();
        UpdateHPBar();
        UpdateWaypointLine();
    }

    // ─────────────────────────────────────────────────────────────
    // PUBLIC COMMAND INTERFACE
    // ─────────────────────────────────────────────────────────────

    public void EnqueueCommand(Command cmd)
    {
        commandQueue.Enqueue(cmd);
        UpdateWaypointLine();
    }

    public void ClearCommands()
    {
        commandQueue.Clear();
        StopAllCoroutines();
        isExecuting    = false;
        currentCommand = null;
        StopMoving();
        State = UnitState.Idle;
        waypointLine.positionCount = 0;
    }

    public int GetQueueCount() => commandQueue.Count + (isExecuting ? 1 : 0);

    public List<Command> GetCommandQueue()
    {
        List<Command> result = new List<Command>();
        if (currentCommand != null) result.Add(currentCommand);
        result.AddRange(commandQueue);
        return result;
    }

    // ─────────────────────────────────────────────────────────────
    // COMMAND EXECUTION
    // ─────────────────────────────────────────────────────────────

    IEnumerator ExecuteCommand(Command cmd)
    {
        isExecuting    = true;
        currentCommand = cmd;

        // ── Communication Delay (core mechanic) ──────────────────
        if (delayIndicator) delayIndicator.SetActive(true);
        yield return new WaitForSeconds(cmd.delay);
        if (delayIndicator) delayIndicator.SetActive(false);

        if (IsDead || GameManager.Instance.IsGameOver) { isExecuting = false; yield break; }

        switch (cmd.type)
        {
            case CommandType.Move:
                yield return StartCoroutine(DoMove(cmd.targetPosition));
                break;
            case CommandType.Attack:
                yield return StartCoroutine(DoAttack(cmd.targetUnit, cmd.targetPosition));
                break;
            case CommandType.Defend:
                yield return StartCoroutine(DoDefend(cmd.targetPosition));
                break;
        }

        currentCommand = null;
        isExecuting    = false;
    }

    // ── MOVE ──────────────────────────────────────────────────────

    IEnumerator DoMove(Vector3 destination)
    {
        State = UnitState.Moving;
        destination.z = 0;
        NavigateTo(destination);

        float timeout = 30f, elapsed = 0f;
        while (elapsed < timeout)
        {
            if (IsDead || GameManager.Instance.IsGameOver) yield break;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMoving();
        State = UnitState.Idle;
    }

    // ── ATTACK ────────────────────────────────────────────────────

    IEnumerator DoAttack(Unit targetUnit, Vector3 fallbackPos)
    {
        State = UnitState.Attacking;

        float maxDuration = 20f, elapsed = 0f;

        while (elapsed < maxDuration)
        {
            if (IsDead || GameManager.Instance.IsGameOver) yield break;

            EnemyUnit enemy      = targetUnit as EnemyUnit;
            bool      targetAlive = enemy != null && !enemy.IsDead;
            Vector3   aimPos     = targetAlive ? targetUnit.transform.position : fallbackPos;
            float     dist       = Vector3.Distance(transform.position, aimPos);

            if (dist > attackRange)
            {
                // Chase into range using NavMesh
                NavigateTo(aimPos);
            }
            else
            {
                StopMoving();

                if (attackTimer <= 0f)
                {
                    FireProjectile(
                        target:    targetAlive ? targetUnit.transform : null,
                        targetPos: aimPos,
                        dmg:       attackDamage,
                        hitsEnemy: true
                    );
                    attackTimer = attackCooldown;
                }

                if (!targetAlive) break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMoving();
        State = UnitState.Idle;
    }

    // ── DEFEND ────────────────────────────────────────────────────

    IEnumerator DoDefend(Vector3 position)
    {
        State = UnitState.Defending;
        position.z = 0;

        // Navigate to defend position
        NavigateTo(position);
        float mt = 0f;
        while (mt < 20f)
        {
            if (IsDead || GameManager.Instance.IsGameOver) yield break;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) break;
            mt += Time.deltaTime;
            yield return null;
        }
        StopMoving();

        // Hold position — shoot any nearby enemy
        float elapsed = 0f;
        while (elapsed < 20f)
        {
            if (IsDead || GameManager.Instance.IsGameOver) yield break;

            EnemyUnit nearby = FindNearestEnemy(attackRange * 1.3f);
            if (nearby != null && attackTimer <= 0f)
            {
                FireProjectile(nearby.transform, nearby.transform.position,
                                attackDamage * 0.85f, hitsEnemy: true);
                attackTimer = attackCooldown;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        State = UnitState.Idle;
    }

    // ─────────────────────────────────────────────────────────────
    // IDLE AUTO-SHOOT
    // ─────────────────────────────────────────────────────────────

    void IdleBehavior()
    {
        State = UnitState.Idle;
        if (attackTimer > 0f) return;

        EnemyUnit e = FindNearestEnemy(attackRange * 0.55f);
        if (e == null) return;

        FireProjectile(e.transform, e.transform.position,
                       attackDamage * 0.5f, hitsEnemy: true);
        attackTimer = attackCooldown * 1.5f;
    }

    // ─────────────────────────────────────────────────────────────
    // FIRE PROJECTILE
    // ─────────────────────────────────────────────────────────────

    void FireProjectile(Transform target, Vector3 targetPos, float dmg, bool hitsEnemy)
    {
        if (projectilePrefab == null)
        {
            // No prefab: instant hit fallback
            if (hitsEnemy && target != null)
                target.GetComponent<EnemyUnit>()?.TakeDamage(dmg);
            return;
        }

        Vector3 spawnPos = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + Vector3.up * 0.1f;

        GameObject go   = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile proj = go.GetComponent<Projectile>();
        if (proj == null) { Destroy(go); return; }

        proj.speed  = projectileSpeed;
        proj.homing = homingShot;

        AudioManager.Instance.PlaySFX(SFX.Fire);

        if (target != null) proj.Fire(target, dmg, hitsEnemy);
        else                 proj.FireAtPosition(targetPos, dmg, hitsEnemy);
    }

    // ─────────────────────────────────────────────────────────────
    // NAVMESH HELPERS
    // ─────────────────────────────────────────────────────────────

    protected void NavigateTo(Vector3 dest)
    {
        if (!agent.enabled) return;
        dest.z = 0f;
        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    protected void StopMoving()
    {
        if (!agent.enabled) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    // ─────────────────────────────────────────────────────────────
    // DAMAGE / DEATH
    // ─────────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHP -= Mathf.RoundToInt(amount);
        if (CurrentHP <= 0) { CurrentHP = 0; Die(); }
    }

    public virtual void Die()
    {
        IsDead = true;
        StopAllCoroutines();
        StopMoving();
        agent.enabled = false;
        StartCoroutine(DeathEffect());
        GameManager.Instance.OnUnitDestroyed();
    }

    protected IEnumerator DeathEffect()
    {
        for (int i = 0; i < 6; i++)
        {
            spriteRenderer.color = Color.white;   yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.red; yield return new WaitForSeconds(0.1f);
        }
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────
    // SELECTION
    // ─────────────────────────────────────────────────────────────

    public void Select()   { IsSelected = true;  spriteRenderer.color = Color.yellow; }
    public void Deselect() { IsSelected = false; UpdateVisuals(); }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────

    EnemyUnit FindNearestEnemy(float range)
    {
        EnemyUnit[] all = FindObjectsOfType<EnemyUnit>();
        EnemyUnit closest = null;
        float minD = range;
        foreach (var e in all)
        {
            if (e.IsDead) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minD) { minD = d; closest = e; }
        }
        return closest;
    }

    protected void UpdateRotation()
    {
        // Only rotate while the NavMesh agent is actively moving
        if (!agent.enabled || agent.isStopped || !agent.hasPath) return;
        if (agent.remainingDistance <= agent.stoppingDistance) return;

        Vector2 velocity = agent.velocity;
        if (velocity.sqrMagnitude < 0.01f) return; // not actually moving yet

        float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        if (rotationSpeed <= 0f)
        {
            // Instant snap
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            // Smooth rotation
            float currentAngle = transform.eulerAngles.z;
            float smoothed = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                                                     rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, smoothed);
        }
    }

    void UpdateVisuals()
    {
        if (IsSelected) return;
        switch (State)
        {
            case UnitState.Idle:      spriteRenderer.color = idleColor;   break;
            case UnitState.Moving:    spriteRenderer.color = idleColor;  break;
            case UnitState.Attacking: spriteRenderer.color = idleColor;  break;
            case UnitState.Defending: spriteRenderer.color = idleColor;  break;
        }
    }

    protected void UpdateHPBar()
    {
        if (hpSlider) hpSlider.value = (float)CurrentHP / maxHP;
    }

    void UpdateWaypointLine()
    {
        List<Vector3> pts = new List<Vector3> { transform.position };
        if (currentCommand != null) pts.Add(currentCommand.targetPosition);
        foreach (var cmd in commandQueue) pts.Add(cmd.targetPosition);
        waypointLine.positionCount = pts.Count;
        waypointLine.SetPositions(pts.ToArray());
    }
}
