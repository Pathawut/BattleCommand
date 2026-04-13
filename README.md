# 🎮 Command Under Fire

A real-time tactical command game where you play as a battlefield **Commander**, issuing orders under pressure with delayed execution.

---

## 🧭 Game Overview

**Genre:** Real-time Strategy (RTS) / Tactical Command (2D Top-down)  
**Platform:** PC (Unity)  
**Playtime:** 5–10 minutes (Demo)

### 🎯 Core Concept
You are not on the battlefield — you are the Commander.

You don’t control units directly.  
You **issue commands**, wait for execution, and deal with the consequences.

---

## 🎯 Core Pillars

- **Delegation Over Control**  
  You command units, not control them directly.

- **Delayed Execution**  
  Every command has a delay (simulating communication lag).

- **Meaningful Consequences**  
  Wrong decisions can result in real losses.

---

## 🔁 Core Gameplay Loop

Observe Battlefield  
→ Decide Unit Production  
→ Wait (Production Delay)  
→ Deploy Units  
→ Issue Commands (with delay)  
→ Battlefield Changes  
→ Repeat  

---

## 🗺️ Game Structure (Demo)

### Objective
- Defend your base from enemy waves  
- Survive for 3 minutes  

### Map Features
- Player Base  
- Enemy Spawn Points (2)  
- Chokepoints  
- Open Combat Areas  

---

## 🧠 Core Mechanics

### 🟦 Command System
- Move  
- Attack  
- Defend  

Commands are queued and executed with delay.

---

### 🟥 Communication Delay
- Every command has a **1–2 second delay**
- Forces players to **think ahead**

---

### 🟨 Unit Types

| Unit   | Strength        | Weakness       | Role       | Build Time |
|--------|----------------|----------------|------------|-----------|
| Heavy  | High HP        | Slow           | Frontline  | 5s        |
| Scout  | Fast           | Low HP         | Recon      | 3s        |
| Sniper | Long Range     | Slow Reload    | Support    | 4s        |

---

### 🟪 AI Behavior
- Idle  
- Moving  
- Attacking  
- Defending  

---

## 🎮 Controls

| Action        | Input        |
|--------------|-------------|
| Select Unit  | Left Click  |
| Move         | Right Click |
| Attack       | UI Button   |
| Defend       | UI Button   |

---

## 🖥️ UI / UX

### Top Bar
- Timer  

### Bottom Panel
- Unit HP / Status  
- Command Buttons  
- Command Queue  

### In-Game Visuals
- Waypoint Lines  
- Command Icons  
- Delay Indicator (⏳)  

---

## 🤖 AI Design

- Move → Follow waypoint  
- Attack → Target nearest enemy  
- Defend → Hold position and fire  

---

## 🎓 Learning Objectives

- Delegation  
- Strategic Thinking  
- Crisis Management  

---

## ⚔️ Difficulty Design

- Wave 1: Basic enemies  
- Wave 2: Heavy units  
- Wave 3: Surround attack  

---

## 🧪 Failure Conditions

- Base destroyed  
- All units lost  

---

## 🏆 Success Conditions

- Survive until time ends  

---

## 🛠️ Technical Design

**Engine:** Unity (2D)

### Systems
- Command Queue  
- Tank AI (FSM)  
- Pathfinding  
- Resource Manager  

---

## 📦 Prototype Scope

### Included
- 1 Map  
- 3 Unit Types  
- Command System  
- Resource System  

### Not Included
- Multiplayer  
- Skill Tree  
- Advanced AI  

---

## 🚀 Future Plans

- Fog of War  
- Multiplayer  
- Advanced AI  
- Upgrade System  

---

## ▶️ How to Run

1. Clone the repo  
2. Open in Unity Hub  
3. Press Play  

---

## 📄 License

Add your license here.
