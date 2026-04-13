# NavMesh + Projectile Setup Guide
## Addendum to SETUP_GUIDE.md

---

## PART A — NavMesh 2D (NavMeshPlus)

Unity's built-in NavMesh only works for 3D. For this 2D top-down game you need
the **NavMeshPlus** package (free, MIT licence).

### A1 — Install NavMeshPlus

**Option 1 — Package Manager (recommended):**
1. Window → Package Manager → **+** → Add package from git URL
2. Paste: `https://github.com/h8man/NavMeshPlus.git#master`
3. Unity will download and import it automatically

**Option 2 — Manual:**
1. Go to https://github.com/h8man/NavMeshPlus/releases
2. Download the latest `.unitypackage`
3. Assets → Import Package → Custom Package

---

### A2 — Add NavMesh Surface to Scene

1. Create an empty GameObject → name it **"NavMeshSurface"**
2. Add Component → **NavMeshSurface** (from NavMeshPlus)
3. Set these values in Inspector:

| Property | Value |
|----------|-------|
| Agent Type | Humanoid (or create a custom 2D agent) |
| Collect Objects | All Game Objects |
| Include Layers | Ground, Default (exclude Enemy/PlayerUnit) |
| Use Geometry | Physics Colliders |
| Override Voxel Size | ✓ → Voxel Size: **0.1** (smaller = more precise paths) |
| Override Tile Size | ✓ → Tile Size: **256** |

4. Click **"Bake"** — you should see the blue NavMesh overlay appear on walkable areas

> **Tip:** Every time you add/move walls, click Bake again to regenerate the mesh.

---

### A3 — Configure NavMesh Agent on Each Unit Prefab

Open each unit prefab (Heavy, Scout, Sniper, BasicEnemy, etc.) and:

1. **Remove** `Rigidbody2D` (no longer needed for movement)
2. **Add Component** → **NavMeshAgent**
3. Set these values:

| Property | Player Units | Enemy Units |
|----------|-------------|-------------|
| Agent Type | Humanoid | Humanoid |
| Base Offset | 0 | 0 |
| Speed | (set by script) | (set by script) |
| Angular Speed | **0** (2D: don't rotate) | **0** |
| Acceleration | 20 | 20 |
| Stopping Distance | 0.15 | ~0.9× attackRange |
| Auto Braking | ✓ | ✓ |
| Obstacle Avoidance | Medium | Low |
| Priority | varies | varies |

4. In the NavMeshAgent component, tick **"Update Rotation: OFF"** and **"Update Up Axis: OFF"** — the Unit.cs script sets these in code too, but double-check here.

---

### A4 — NavMesh Obstacles (Walls)

For each Wall prefab:
1. Add Component → **NavMeshObstacle**
2. Shape: **Box**
3. Size: match your wall collider size
4. **Carve: ✓** (this cuts a hole in the NavMesh dynamically)

> With Carve enabled, walls automatically update the NavMesh at runtime — no need to re-bake when walls appear/disappear.

---

### A5 — Ground Layer Setup

The NavMesh needs to know what's walkable:
1. Select your ground/floor sprite GameObject
2. Set its Layer to **"Ground"**
3. Make sure it has a **BoxCollider2D** (NavMesh uses colliders to detect walkable surface)
4. Re-bake NavMesh

---

### A6 — Test NavMesh

Press Play, select a unit, right-click somewhere.
You should see the unit's NavMeshAgent draw a path and the unit walks around walls.
If it goes through walls: check the wall has a Collider2D and NavMeshObstacle.

---

## PART B — Projectile Setup

### B1 — Create the Projectile Prefab

**Player projectile (yellow shell):**
1. Create empty GameObject → name `"Projectile_Player"`
2. Add **SpriteRenderer** → use a small circle/square sprite
   - Color: `(1, 0.9, 0, 1)` — bright yellow
   - Scale: `(0.15, 0.15, 1)`
   - Order in Layer: **10** (renders above units)
3. Add **CircleCollider2D** → Is Trigger: **YES**, Radius: **0.08**
4. Add **Rigidbody2D** → Gravity Scale: **0**, Collision Detection: **Continuous**
5. Add **Projectile** script (Scripts/Combat/Projectile.cs)
6. Drag to Prefabs/Combat folder → delete scene instance

**Enemy projectile (red bolt):**
- Same setup but Color: `(1, 0.2, 0.2, 1)` — red
- Name: `"Projectile_Enemy"`

**Sniper round (long white bolt):**
- Scale: `(0.3, 0.08, 1)` — thin and long
- Color: `(1, 1, 1, 1)` — white
- Projectile speed: **14** (fast)
- Name: `"Projectile_Sniper"`

---

### B2 — Create Impact FX (Optional but satisfying)

**Simple approach — use a Particle System:**
1. Create → Effects → Particle System → name `"ImpactFX"`
2. Configure:
   - Duration: 0.3, Looping: OFF, Start Lifetime: 0.2
   - Start Speed: 2, Start Size: 0.2
   - Max Particles: 8
   - Emission: Burst → Count 6 at Time 0
   - Renderer → Sorting Layer: UI, Order in Layer: 15
3. Save as prefab → assign to Projectile's `defaultImpactFX` slot

---

### B3 — Assign Projectile Prefabs to Units

Open each Unit prefab in Inspector:

| Unit | Projectile Prefab | Speed | Homing |
|------|------------------|-------|--------|
| Heavy Tank | Projectile_Player | 7 | false |
| Scout | Projectile_Player | 10 | false |
| Sniper | Projectile_Sniper | 14 | false |
| Basic Enemy | Projectile_Enemy | 6 | false |
| Heavy Enemy | Projectile_Enemy | 5 | false |
| Flanker | Projectile_Enemy | 8 | false |

**Muzzle Point (optional):**
1. In the unit prefab, create a child empty GameObject
2. Name it `"MuzzlePoint"`, position it at the front of the sprite (e.g. `x = 0.3`)
3. Assign this transform to the Unit's `Muzzle Point` field

---

### B4 — Projectile Layer Collision

In **Edit → Project Settings → Physics 2D**, make sure:
- `Projectile_Player` layer hits: `Enemy`, `Wall`
- `Projectile_Enemy` layer hits: `PlayerUnit`, `Wall`, `PlayerBase`

Create layers:
- Layer 9: `ProjectilePlayer`
- Layer 10: `ProjectileEnemy`

Assign to prefabs, then in the collision matrix:

| | PlayerUnit | Enemy | Wall | PlayerBase |
|-|-----------|-------|------|------------|
| ProjectilePlayer | ✗ | ✓ | ✓ | ✗ |
| ProjectileEnemy  | ✓ | ✗ | ✓ | ✓ |

---

## PART C — Architecture Summary (Updated)

```
Unit (NavMeshAgent)
├── EnqueueCommand(cmd)
│     └── [Communication Delay: 1.5s]
│           ├── DoMove()     → agent.SetDestination()
│           ├── DoAttack()   → chase via NavMesh → FireProjectile()
│           └── DoDefend()   → navigate → hold + FireProjectile()
│
└── FireProjectile()
      └── Instantiate(projectilePrefab)
            └── Projectile.Fire(target, damage, hitsEnemy)
                  ├── Rigidbody2D.velocity → flies to target
                  ├── OnTriggerEnter2D → TakeDamage() on hit
                  └── SpawnImpact() → particle fx

EnemyUnit (NavMeshAgent)
├── RunFSM()
│     ├── Idle  → wait
│     ├── Moving → agent.SetDestination(nearest player/base)
│     └── Attacking → FireProjectile() → Projectile(hitsEnemy=false)
```

---

## PART D — Common Issues

| Problem | Cause | Fix |
|---------|-------|-----|
| Unit doesn't move | NavMesh not baked | Click Bake on NavMeshSurface |
| Unit walks through walls | Wall missing NavMeshObstacle | Add NavMeshObstacle + Carve |
| "Failed to create agent" error | Unit not on NavMesh | Move unit onto baked area, check Ground layer |
| Projectile goes wrong direction | `muzzlePoint` offset wrong | Remove muzzle child or reset its position |
| Projectile hits own unit | Layer collision wrong | Fix Physics2D collision matrix |
| Agent spins in 2D | updateRotation not false | Set in Inspector AND confirmed in Awake() |
| Agent sinks to z=-10 | updateUpAxis not false | NavMeshPlus: set updateUpAxis = false in code |

---

## Quick Checklist

- [ ] Install NavMeshPlus via Package Manager
- [ ] Add NavMeshSurface component to scene, set Include Layers = Ground
- [ ] Bake NavMesh (see blue overlay)
- [ ] Add NavMeshObstacle (Carve) to all walls
- [ ] Remove Rigidbody2D from unit prefabs
- [ ] Add NavMeshAgent to unit prefabs, set updateRotation=false, updateUpAxis=false
- [ ] Create Projectile_Player prefab (circle, yellow, trigger collider, Projectile.cs)
- [ ] Create Projectile_Enemy prefab (circle, red, trigger collider, Projectile.cs)
- [ ] Assign projectile prefabs to each unit prefab
- [ ] Set up layer collision matrix for projectiles
- [ ] Press Play → units pathfind, projectiles fly ✅
