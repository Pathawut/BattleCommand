using UnityEngine;

/// <summary>
/// A physical projectile spawned when a unit fires.
/// Travels toward its target (or a fixed world position for lobbed shots).
/// On hit: deals damage, spawns an impact effect, then destroys itself.
///
/// HOW TO CREATE THE PREFAB:
///   1. New empty GameObject → name "Projectile"
///   2. Add SpriteRenderer   → assign a small circle/square sprite, scale ~(0.15, 0.15, 1)
///   3. Add CircleCollider2D → Is Trigger: YES, radius ~0.1
///   4. Add THIS script
///   5. Save as Prefab → assign to each Unit's projectilePrefab slot
///
/// Per-unit colour variants (optional):
///   Heavy  → yellow shell     (Color: 1, 0.9, 0,   1)
///   Scout  → green tracer     (Color: 0.3, 1, 0.3, 1)
///   Sniper → bright white bolt(Color: 1,   1, 1,   1)  scale wider/thinner
///   Enemy  → red bolt         (Color: 1, 0.2, 0.2, 1)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    // ── Set by the firing unit via Fire() ─────────────────────────
    private Transform  targetTransform;   // if tracking a moving target
    private Vector3    targetPosition;    // fallback fixed position
    private float      damage;
    private bool       hitsEnemy;         // true = damages EnemyUnit, false = damages Unit
    private GameObject impactFXPrefab;    // optional: explosion/spark prefab

    // ── Config (set in Inspector on the prefab) ───────────────────
    [Header("Flight")]
    public float speed       = 8f;
    public float maxLifetime = 4f;       // auto-destroy safety net
    public bool  homing      = false;    // if true, steers toward moving target

    [Header("Impact FX (optional)")]
    public GameObject defaultImpactFX;  // small particle burst prefab

    // Runtime
    private Rigidbody2D rb;
    private float lifetime;
    private bool  hasHit;

    // ─────────────────────────────────────────────────────────────
    // INITIALISE — called immediately after Instantiate
    // ─────────────────────────────────────────────────────────────
    public void Fire(Transform target, float dmg, bool enemyProjectile,
                     GameObject fxOverride = null)
    {
        targetTransform = target;
        targetPosition  = target != null ? target.position : transform.position;
        damage          = dmg;
        hitsEnemy       = enemyProjectile;
        impactFXPrefab  = fxOverride != null ? fxOverride : defaultImpactFX;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Ensure trigger
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        // Initial velocity toward target
        SetVelocityToward(targetPosition);
        RotateTowardVelocity();
    }

    // Overload: fire toward a fixed world position (e.g. base attack)
    public void FireAtPosition(Vector3 pos, float dmg, bool enemyProjectile,
                               GameObject fxOverride = null)
    {
        targetTransform = null;
        targetPosition  = pos;
        damage          = dmg;
        hitsEnemy       = enemyProjectile;
        impactFXPrefab  = fxOverride != null ? fxOverride : defaultImpactFX;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        SetVelocityToward(targetPosition);
        RotateTowardVelocity();
    }

    // ─────────────────────────────────────────────────────────────

    void Update()
    {
        if (hasHit) return;

        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime) { DestroyProjectile(); return; }

        // Homing: steer toward moving target each frame
        if (homing && targetTransform != null)
        {
            targetPosition = targetTransform.position;
            SetVelocityToward(targetPosition);
        }

        // Always rotate to face current velocity direction
        RotateTowardVelocity();

        // Check arrival (for non-homing or fixed-position shots)
        if (!homing && Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            HitPosition();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // COLLISION
    // ─────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (hitsEnemy)
        {
            // Projectile fired by player → damages EnemyUnit
            EnemyUnit enemy = other.GetComponent<EnemyUnit>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
                SpawnImpact();
                hasHit = true;
                DestroyProjectile();
                return;
            }

            // Also damage enemy base
            Base b = other.GetComponent<Base>();
            if (b != null && !b.isPlayerBase)
            {
                b.TakeDamage(damage);
                SpawnImpact();
                hasHit = true;
                DestroyProjectile();
                return;
            }
        }
        else
        {
            // Projectile fired by enemy → damages player Unit
            Unit unit = other.GetComponent<Unit>();
            if (unit != null && !(unit is EnemyUnit) && !unit.IsDead)
            {
                unit.TakeDamage(damage);
                SpawnImpact();
                hasHit = true;
                DestroyProjectile();
                return;
            }

            // Also damage player base
            Base b = other.GetComponent<Base>();
            if (b != null && b.isPlayerBase)
            {
                b.TakeDamage(damage);
                SpawnImpact();
                hasHit = true;
                DestroyProjectile();
                return;
            }
        }

        // Hit a wall
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            SpawnImpact();
            hasHit = true;
            DestroyProjectile();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────

    void HitPosition()
    {
        SpawnImpact();
        hasHit = true;
        DestroyProjectile();
    }

    void SetVelocityToward(Vector3 pos)
    {
        if (rb == null) return;
        Vector2 dir = (pos - transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    void RotateTowardVelocity()
    {
        if (rb == null) return;
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void SpawnImpact()
    {
        if (impactFXPrefab != null)
        {
            AudioManager.Instance.PlaySFX(SFX.Bomb);
            GameObject fx = Instantiate(impactFXPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }
        else
        {
            // Fallback: quick white flash sprite
            StartCoroutine(FlashImpact());
        }
    }

    System.Collections.IEnumerator FlashImpact()
    {
        GameObject flash = new GameObject("Impact");
        flash.transform.position = transform.position;
        var sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>()?.sprite;
        sr.color  = Color.white;
        flash.transform.localScale = Vector3.one * 0.4f;

        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            sr.color = new Color(1, 1, 1, 1 - t / 0.15f);
            flash.transform.localScale = Vector3.one * (0.4f + t * 2f);
            yield return null;
        }
        Destroy(flash);
    }

    void DestroyProjectile()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }
}
