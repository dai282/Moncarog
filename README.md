# 🐉 Moncarog: A Roguelike Creature Battler

**Moncarog** is a roguelike monster-catching adventure built in **Unity**. Explore procedurally generated maps, battle mysterious creatures called **Moncargs**, and build your dream team as you delve deeper into the unknown.

---

## 🧭 Table of Contents
1.  [Overview](#-overview)
2.  [Screenshots](#-screenshots)
3.  [Gameplay Features](#️-gameplay-features)
4.  [Core Systems](#-core-systems)
5.  [Development Setup](#-development-setup)
6.  [Branching & Version Control](#-branching--version-control)
7.  [Future Plans](#-future-plans)
8.  [Credits](#-credits)
9.  [License](#-license)
---

## 🌍 Overview

**Moncarog** combines the thrill of roguelike exploration with strategic, turn-based combat. Each run offers new challenges, enemies, and rewards — no two playthroughs are ever the same.

You’ll encounter **wild Moncargs**, engage them in tactical battles, and attempt to **catch and recruit** them to your party. Victory depends on smart decisions, resource management, and mastering elemental types.

---
## 📸 Screenshots

<img width="591" height="329" alt="image" src="https://github.com/user-attachments/assets/21bbdec0-dbbc-45b1-93dc-2833d3094c4e" />

<img width="591" height="329" alt="image" src="https://github.com/user-attachments/assets/222c755d-1ccd-4b20-b1ff-598d226b5d74" />

<img width="591" height="329" alt="image" src="https://github.com/user-attachments/assets/c2138549-192b-4175-bb6c-3a369f9f113c" />

<img width="591" height="329" alt="image" src="https://github.com/user-attachments/assets/f0a62d63-b052-4df2-8079-50a7a2339b1c" />

<img width="591" height="329" alt="image" src="https://github.com/user-attachments/assets/dfd4e30f-a06f-4e87-b788-65980b34efd7" />

---

## ⚔️ Gameplay Features

-   🗺️ **Procedurally generated maps** — every run is unique
-   💥 **Turn-based combat** with elemental strengths and weaknesses
-   🎯 **Creature catching** inspired by classic monster games
-   ⚙️ **Stat-driven Moncargs** — health, mana, speed, attack, defense, etc.
-   🧠 **Smart enemy AI** using weighted combat logic
-   🧩 **Modular systems** for scalable development
-   🎨 **UI Toolkit-based interfaces** for clean, dynamic menus

---

## 🧱 Core Systems

| System          | Description                                                    |
| --------------- | -------------------------------------------------------------- |
| **Moncarg**     | Creature blueprint storing stats, skills, and role (Player or Enemy). |
| **Skill**       | Defines moves with elemental types, damage, and mana cost.     |
| **CombatHandler**| Manages encounters, turn order, skill execution, and AI behavior. |
| **GameManager** | Oversees spawning, scene flow, and global systems.             |
| **BoardManager** | Generates room based on map location             |
| **MapManager** | Generates Map             |
| **PlayerInventory** | Inventory System, holds Moncarg and Item Scriptable Objects             |

---

## 🧩 Development Setup

**Engine:** Unity `6000.2.1f1`
**Language:** C#

### 🧱 To Run Locally (in Unity)

1.  Clone this repository:
    ```bash
    git clone https://github.com/dai282/Moncarog.git
    ```
2.  Open the project in **Unity Hub**.
3.  Load the **Main Scene** (under `Assets/Scenes/`).
4.  Press **Play** to start a test encounter.

---

## 🌿 Branching & Version Control

Each feature is developed in its own branch and corresponding scene:

| Branch     | Purpose                               |
| ---------- | ------------------------------------- |
| `Player/`  | Player movement and inventory     |
| `Moncarg/` | Creature systems and combat mechanics |
| `Board/`      | Room generation       |

**Workflow:**

1.  Update your branch with the latest from `main`.
2.  Integrate your feature scene with `Main` locally.
3.  Merge into `main` once the scene and systems are stable.

> 💡 *Avoid editing shared scenes directly — use prefabs or additive scenes to minimize merge conflicts.*

---

## 🚀 Future Plans

-   🧬 Expand Moncarg roster and elemental interactions
-   🎭 Add skill animations, effects, and combat feedback
-   🏰 Implement dungeon exploration loop
-   💾 Introduce save/load functionality
-   📜 Migrate SkillList and Moncarg data to ScriptableObjects
-   🌐 (Optional) Multiplayer encounters

---

## 👥 Credits

| Role                          | Contributor |
| ----------------------------- | ----------- |
| **Game Design / Programming** | Dai Nguyen, Niko Goijarts, Joseph Nicholls, Robert George William Sevele, Cooper Gardyne  |
| **Art / UI Design**           | Joseph Nicholls, Niko Goijarts, Dai Nguyen          |
| **Sound & Music**             | Niko Goijarts, Dai Nguyen          |
| **Special Thanks**            | Auckland University of Technology COMP602 Teaching Team           |

---

## 📜 License

This project is for educational and portfolio use only.
All rights reserved © **(2025)** **Moncarog Team**.
