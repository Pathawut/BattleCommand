# Command Under Fire — Unity Setup Guide
## Complete Step-by-Step Implementation

---

## 📁 Project Folder Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── CommandSystem.cs
│   │   ├── WaveManager.cs
│   │   ├── Base.cs
│   │   ├── MapGenerator.cs
│   │   ├── CameraController.cs
│   │   └── ResourceDrop.cs
│   ├── Units/
│   │   ├── Unit.cs
│   │   ├── HeavyUnit.cs
│   │   ├── ScoutSniperUnit.cs  (contains Scout + Sniper)
│   │   └── EnemyUnit.cs
│   ├── Input/
│   │   └── PlayerController.cs
│   └── UI/
│       └── UnitPanelUI.cs
├── Prefabs/
│   ├── Units/
│   │   ├── HeavyUnit.prefab
│   │   ├── ScoutUnit.prefab
│   │   ├── SniperUnit.prefab
│   │   ├── BasicEnemy.prefab
│   │   ├── HeavyEnemy.prefab
│   │   └── FlankerEnemy.prefab
│   ├── Map/
│   │   ├── PlayerBase.prefab
│   │   ├── EnemyHQ.prefab
│   │   ├── Wall.prefab
│   │   └── Ground.prefab
│   └── UI/
│       └── QueueSlot.prefab
├── Scenes/
│   └── GameScene.unity
└── Sprites/   ← your pixel art goes here
```

---

## 🔧 STEP 1 — Unity Project Setup

1. Create a new **Unity 2D** project (Unity 6 or 2022 LTS recommended)
2. Install **TextMeshPro** via Package Manager → it will ask to import TMP Essentials, do it
3. Set camera to **Orthographic**, size = 7
4. Copy all `.cs` files into `Assets/Scripts/` matching the folder structure above

---

## 🎨 STEP 2 — Create Sprites (Simple Placeholders)

Create simple colored squares/circles using Unity's built-in sprite shapes:

| Unit        | Shape  | Color          |
|-------------|--------|----------------|
| Heavy Tank  | Square | Blue-grey      |
| Scout       | Small circle | Green    |
| Sniper      | Tall rect | Orange-gold |
| Basic Enemy | Square | Red            |
| Heavy Enemy | Big square | Dark red  |
| Flanker     | Diamond | Orange        |
| Player Base | Big square | Blue       |
| Enemy HQ    | Big square | Dark red   |
| Wall        | Rectangle | Grey       |

**To make a sprite in Unity:**
- Right-click in Project → Create → Sprites → Square (or Circle)
- Drag to scene, set SpriteRenderer color in Inspector

---

## 🏗️ STEP 3 — Create Prefabs

### A) Unit Prefab Setup (do for each unit type)

1. Create empty GameObject → rename "HeavyUnit"
2. Add components:
   - `SpriteRenderer` — assign your Heavy sprite
   - `Rigidbody2D` — Gravity Scale: 0, Freeze Rotation Z: ✓
   - `CircleCollider2D` — Is Trigger: NO (for physics)
   - `HeavyUnit` script
   - `Canvas` (World Space) → add `Slider` (HP bar)
3. Set Layer to **"PlayerUnit"** (create this layer)
4. Drag to Prefabs/Units folder

### B) Enemy Prefab Setup

1. Same as above but add `EnemyUnit` script instead
2. Set Layer to **"Enemy"**

### C) Base Prefab

1. SpriteRenderer (big square)
2. `BoxCollider2D`
3. `Base` script
4. Canvas → HP Slider + Text

### D) Wall Prefab

1. SpriteRenderer (grey rectangle)
2. `BoxCollider2D` (Is Trigger: NO)

---

## 🏛️ STEP 4 — Scene Setup

### 4.1 — Create Empty GameObjects (hierarchy):

```
Scene Hierarchy:
├── --- MANAGERS ---
│   ├── GameManager       [GameManager.cs]
│   ├── CommandSystem     [CommandSystem.cs]
│   └── WaveManager       [WaveManager.cs]
├── --- CAMERA ---
│   └── Main Camera       [CameraController.cs]
├── --- INPUT ---
│   └── PlayerController  [PlayerController.cs]
├── --- MAP ---
│   └── MapGenerator      [MapGenerator.cs]
└── --- UI ---
    └── Canvas            [CanvasUI]
```

### 4.2 — Physics Layers

Go to **Edit → Project Settings → Tags and Layers**, create:
- Layer 6: `PlayerUnit`
- Layer 7: `Enemy`
- Layer 8: `Ground`

In **Edit → Project Settings → Physics 2D**, configure collision matrix:
- PlayerUnit vs Enemy: ✓ (they can collide)
- PlayerUnit vs Ground: ✓

### 4.3 — PlayerController Layer Masks

In PlayerController Inspector:
- Unit Layer: `PlayerUnit`
- Ground Layer: `Ground`
- Enemy Layer: `Enemy`

---

## 🖥️ STEP 5 — UI Setup

Create a Canvas (Screen Space - Overlay):

### Top Bar:
```
Canvas/TopBar (horizontal layout)
├── TimerText      [TextMeshProUGUI] — "02:00"
├── AmmoText       [TextMeshProUGUI] — "Ammo: 100"
├── FuelText       [TextMeshProUGUI] — "Fuel: 100"
└── WaveText       [TextMeshProUGUI] — "Wave 1"
```

### Left Panel (Unit Panel):
```
Canvas/LeftPanel
├── UnitPanelUI component attached here
├── UnitNameText   [TextMeshProUGUI]
├── UnitTypeText   [TextMeshProUGUI]
├── UnitStatusText [TextMeshProUGUI]
├── UnitHPSlider   [Slider]
├── QueueContainer [Vertical Layout Group] ← queue slots spawn here
└── Buttons:
    ├── MoveButton    "Move [RMB]"
    ├── AttackButton  "Attack [A]"
    ├── DefendButton  "Defend [D]"
    ├── ClearButton   "Clear Queue"
    ├── RefillAmmoBtn "Refill Ammo (-30)"
    └── RefillFuelBtn "Refill Fuel (-30)"
```

### Bottom Panel:
Already wired through UnitPanelUI buttons above.

### Overlay Panels:
```
Canvas/VictoryPanel  (disabled by default)
├── Title: "VICTORY!"
└── RestartButton

Canvas/DefeatPanel   (disabled by default)
├── Title: "DEFEATED"
└── RestartButton
```

**Wire RestartButton.onClick → GameManager.RestartGame()**

---

## 🌊 STEP 6 — Wave Manager Configuration

Select WaveManager in hierarchy, configure Waves array:

```
Wave 0 (Wave 1 — Basic):
  waveName: "Wave 1 - Scouts"
  startDelay: 5
  basicCount: 3
  heavyCount: 0
  flankerCount: 0
  spawnInterval: 2

Wave 1 (Wave 2 — Heavy):
  waveName: "Wave 2 - Heavy Push"
  startDelay: 8
  basicCount: 2
  heavyCount: 2
  flankerCount: 0
  spawnInterval: 2.5

Wave 2 (Wave 3 — Surround):
  waveName: "Wave 3 - Surround!"
  startDelay: 10
  basicCount: 2
  heavyCount: 1
  flankerCount: 3
  spawnInterval: 1.5
```

Set Spawn Points:
- Create 2 empty GameObjects at top-left and top-right of map
- Assign to WaveManager's SpawnPoints array

---

## 🗺️ STEP 7 — Map Generator Configuration

Select MapGenerator, assign prefabs in Inspector:
- Player Base Prefab
- Enemy HQ Prefab  
- Wall Prefab
- Ground Prefab
- Heavy/Scout/Sniper Unit Prefabs

Map Width: 20, Map Height: 14

---

## 🔗 STEP 8 — Wire Up GameManager

Select GameManager, assign all UI references:
- Timer Text
- Ammo Text
- Fuel Text
- Mission Text
- Player Base (find in scene)
- Enemy HQ (find in scene)
- Wave Manager
- Victory Panel
- Defeat Panel

---

## 🎮 STEP 9 — Controls Summary

| Action | Input |
|--------|-------|
| Select Unit | Left Click on unit |
| Move | Right Click on ground |
| Attack | Right Click on enemy (or [A] then click) |
| Defend | [D] then click location |
| Pan Camera | WASD / Arrow Keys / Mouse edge |
| Zoom | Mouse Scroll Wheel |
| Clear Queue | Escape / Clear Button |

---

## ⚙️ STEP 10 — Command Delay Tuning

In CommandSystem Inspector:
- **Command Delay: 1.5s** (default — this is the core mechanic!)
- Try 1.0s for easier, 2.5s for hard mode

---

## 🐛 Common Issues & Fixes

| Problem | Fix |
|---------|-----|
| Units don't move | Check Rigidbody2D gravity = 0, Layer assigned |
| Units clip through walls | Ensure BoxCollider2D on walls, not trigger |
| Click goes through UI | EventSystem in scene? PlayerController checks IsPointerOverGameObject |
| Units attack each other | Check layer masks on PlayerController |
| No HP bars visible | Set HP Slider Canvas to World Space, scale ~0.01 |

---

## 🚀 Quick Prototype Checklist

- [ ] Copy all .cs scripts into project
- [ ] Create 3 player unit prefabs (Heavy, Scout, Sniper)
- [ ] Create 3 enemy prefabs (Basic, Heavy, Flanker)
- [ ] Create Player Base + Enemy HQ prefabs
- [ ] Create Wall prefab
- [ ] Set up layers (PlayerUnit, Enemy, Ground)
- [ ] Build Canvas UI with all panels
- [ ] Configure GameManager references
- [ ] Configure WaveManager waves + spawn points
- [ ] Set camera to Orthographic, add CameraController
- [ ] Add PlayerController to empty GameObject
- [ ] Press Play and test!

---

## 📐 Architecture Overview

```
GameManager
├── CommandSystem ────► Unit.EnqueueCommand()
│                           └── [delay] → Execute (Move/Attack/Defend)
├── WaveManager ──────► Spawns EnemyUnit[]
│                           └── FSM: Idle → Chase → Attack
├── PlayerController ─► Detects click → IssueCommand()
├── UnitPanelUI ──────► Reflects selected unit state
└── ResourceManager ──► Ammo / Fuel global pool
```

---

*Good luck, Commander. Every decision counts.*
