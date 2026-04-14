# 🎮 BATTLE COMMAND

**Real-Time Strategy • Tactical Command • 2D Top-Down**

---

## 📌 Overview

**Battle Command** is a real-time strategy game where *you are the Commander — not a soldier.*

You never directly control units. Instead, you issue commands with a **communication delay**, forcing you to think ahead and adapt to a constantly changing battlefield.

> ⚠️ Even perfect orders can fail — timing is everything.

---

## 🧠 Core Design Pillars

- **Delegation over control** — you command, units act
- **Communication delay** — orders take ~1.5 seconds to execute
- **Meaningful consequences** — mistakes cost real unit lives

---

## 🎯 Mission Objectives

### ✅ Victory Conditions
- Survive for **2-3 minutes**
- OR eliminate all enemy units

### ❌ Defeat Conditions
- Player Base HP reaches zero
- All player units are destroyed

---

## 🎮 How to Play

### Gameplay Loop

1. Observe the battlefield
2. Select a unit *(Left Click)*
3. Issue a command:
   - Move → Right Click
   - Attack → `[A]` + Click enemy
   - Defend → `[D]` + Click position
4. Wait for delay (~1.5s)
5. Adapt to the new situation
6. Survive or eliminate enemies

---

## 🕹 Controls

### Unit Commands

| Action        | Input                     | Notes |
|--------------|--------------------------|------|
| Select Unit  | Left Click               | Highlights yellow |
| Move         | Right Click              | Uses NavMesh pathfinding |
| Attack       | `[A]` + Click enemy      | Moves into range and fires |
| Defend       | `[D]` + Click position   | Holds position, auto-attacks |
| Clear Queue  | `Esc`                    | Cancels pending commands |
| Deselect     | Click empty ground       | — |

---

## 🏭 Unit Production

| Unit        | Hotkey | Build Time | Notes |
|------------|--------|------------|------|
| Heavy Tank | `[1]`  | 5 sec      | Strong frontline |
| Scout      | `[2]`  | 2 sec      | Fast, queue up to 5 |
| Sniper     | `[3]`  | 4 sec      | Long-range support |

- Units spawn near the Player Base
- Production runs in the background

---

## 🚀 Unit Roster

| Unit        | HP  | Speed  | Range     | Role |
|------------|-----|--------|----------|------|
| Heavy Tank | 200 | Slow   | Short     | Frontline tank |
| Scout      | 60  | Fast   | Medium    | Flanker / interceptor |
| Sniper     | 80  | Medium | Very Long | Backline damage |

### ⚔️ Symmetric Forces
Enemies use the **same units** as you.  
Learn counters — they apply both ways.

---

## 🌊 Enemy Waves

| Wave | Name            | Heavy | Scout | Sniper |
|------|----------------|-------|-------|--------|
| 1    | Basic Push     | 1     | 2     | 0      |
| 2    | Heavy Assault  | 2     | 1     | 1      |
| 3    | Surround Attack| 1     | 2     | 2      |

- Spawn from **two map corners**
- Wave 3 introduces **flanking attacks**

---

## 🧩 Strategy Tips

- Hold chokepoints with **Heavy Tanks**
- Queue commands early (due to delay)
- Use **Defend near base** for automation
- Prioritize **enemy Snipers first**
- Counter Scouts with your own Scouts
- Keep building units during combat
- Replace losses quickly with Scouts (2s build time)

---

## 🤖 AI-Assisted Development

This project was built using a **human-led, AI-assisted workflow**.

---

### 🧠 ChatGPT (OpenAI)
- Game design & concept development
- Defined gameplay pillars and systems
- Generated concept art references

---

### 🧑‍💻 Claude (Anthropic)
- Generated all **C# Unity scripts**
- Built core systems:
  - GameManager
  - CommandSystem
  - WaveManager
  - Unit / EnemyUnit
  - ProductionSystem
- Created documentation and setup guides

---

### 🎨 Gemini (Google)
- Pixel-art sprites (units, map, effects)
- Sound design (BGM + SFX)

---

### 🔄 Workflow Summary

| Phase            | Tool     | Contribution |
|------------------|----------|-------------|
| Game Design      | ChatGPT  | Core ideas & mechanics |
| Concept Art      | ChatGPT  | Visual references |
| Code & Docs      | Claude   | Full implementation |
| Final Assets     | Gemini   | Sprites & visuals |
| Sound Design     | Gemini   | Music & SFX |

---

## 🖼 Screenshot

![Screenshot 2026-04-14 141108](https://github.com/user-attachments/assets/5563112e-dc88-4900-85df-705224570e38)

![Screenshot 2026-04-14 141329](https://github.com/user-attachments/assets/f915bf24-adac-4bf7-9dd3-62afc9c9adf4)


---

## ⚙️ Tech Stack

- **Engine:** Unity
- **Language:** C#
- **Navigation:** NavMeshPlus (2D)

---

## 📦 Demo Info

- ⏱ Length: 2-3 minutes  
- 🗺 Map: 1  
- 🌊 Waves: 3  
- 👤 Mode: Single-player  

---

## 🏁 Credits

Built with:
- Unity
- Claude
- ChatGPT
- Gemini

---
