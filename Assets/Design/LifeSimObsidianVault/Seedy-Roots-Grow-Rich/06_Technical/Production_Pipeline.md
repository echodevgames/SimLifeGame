# Production Pipeline

## Links

**Depends On:**
- [[Technical_Architecture]]

**Used By:**
- (Development Planning)

---

## Overview

This document defines the high-level development pipeline for the project.

It outlines:

- System “departments”
- Dependencies between systems
- Core tasks for each system
- Broad development phases

---

## 🧱 Core Development Layers

Development is divided into layered systems:

1. Foundation Systems
2. Core Gameplay Systems
3. Progression Systems
4. Content Systems
5. UI Systems
6. Polish & Expansion

---

# 🏗️ Phase 1 — Foundation Systems

## Systems

- GameStateManager
- Event System (core architecture)
- Basic Player Controller

---

## Tasks

- Create base project structure
- Implement event system
- Set up core managers
- Establish scene + basic player interaction

---

## Goal

👉 A running project with core architecture in place

---

# 🌱 Phase 2 — Core Gameplay Systems

## Systems

- Grow System (CORE)
- Needs System
- Economy System

---

## Dependencies

- Event system must exist
- Core managers must be initialized

---

## Tasks

- Implement GrowManager and GrowSetup
- Create production cycle logic
- Implement needs decay + effects
- Connect needs → production efficiency
- Implement basic money flow

---

## Goal

👉 Player can:
- Place a grow setup
- Generate income
- Be affected by needs

---

# 📋 Phase 3 — Progression Systems

## Systems

- Mission System
- Unlock System
- Progression System

---

## Dependencies

- Grow System
- Economy System
- Needs System

---

## Tasks

- Implement MissionManager
- Create mission data (ScriptableObjects)
- Hook events into mission progress
- Implement UnlockManager
- Gate content behind progression

---

## Goal

👉 Player has:
- Objectives
- Rewards
- Structured progression

---

# 🧱 Phase 4 — Content Systems

## Systems

- Items
- Locations
- Unlockables

---

## Dependencies

- Progression System
- Grow System

---

## Tasks

- Create item data (ScriptableObjects)
- Define placement rules
- Implement location scaling
- Connect unlocks to content

---

## Goal

👉 Player can:
- Expand space
- Place meaningful items
- Unlock new content

---

# 🧍 Phase 5 — Social Systems

## Systems

- Social System
- NPC interactions

---

## Dependencies

- Needs System
- Mission System

---

## Tasks

- Implement relationship tracking
- Create interaction system
- Connect social → missions
- Connect social → progression

---

## Goal

👉 Player can:
- Build relationships
- Unlock new opportunities

---

# 🖥️ Phase 6 — UI Systems

## Systems

- Mission UI
- Needs UI
- Production UI

---

## Dependencies

- All gameplay systems

---

## Tasks

- Display mission progress
- Display needs status
- Display production output
- Hook UI into event system

---

## Goal

👉 Player can clearly:
- Understand systems
- Track progress
- Make decisions

---

# ✨ Phase 7 — Polish & Expansion

## Systems

- Balance systems
- Add content
- Improve feedback

---

## Tasks

- Tune production values
- Improve UI feedback
- Add more items/locations
- Expand mission variety

---

## Goal

👉 Game feels complete and responsive

---

# 🔗 System Dependency Overview

## Core Dependency Flow

[Event System]
      ↓
[Grow System]   [Needs System]
        ↓             ↓
        └────→ [Economy System]
                    ↓
              [Mission System]
                    ↓
              [Unlock System]
                    ↓
        [Items]   [Locations]
                    ↓
                   [UI]