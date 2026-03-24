# Grow System

## Links

**Depends On:**
- [[Needs_System]]
- [[Economy_System]]
- [[Items]]

**Used By:**
- [[Career_System]]
- [[Progression_System]]
- [[Mission_System_Overview]]
- [[Core_Gameplay_Loop]]
- [[UI_Overview]]

---

## Overview

The Grow System is the primary gameplay system that replaces traditional job mechanics.

Players generate income by:

- Setting up grow environments
- Maintaining production systems
- Optimizing efficiency and layout
- Expanding their operation over time

---

## Core Loop

Setup → Maintain → Produce → Collect → Sell → Upgrade → Expand

---

## Core Components

### 🧱 Grow Setup

A grow setup consists of:

- Grow Space (closet, room, basement, etc.)
- Equipment (lights, containers, systems)
- Layout (positioning and density)

Each setup produces output over time.

---

### ⏱️ Production Cycle

Production occurs over time in cycles.

Each cycle:

1. Starts when setup is active
2. Progresses automatically
3. Completes after a duration
4. Generates output (income or product)

---

### ⚙️ Efficiency Factors

Production output is affected by:

- Equipment quality
- Layout optimization
- Player interaction
- Needs state (energy, etc.)
- Space constraints

---

### 🧍 Player Interaction

The player can:

- Set up new grow areas
- Adjust layout and equipment
- Perform maintenance actions
- Expand into new spaces

---

## Expansion System

Grow setups scale through space progression:

- Closet → Small Room → Basement → Full House → Warehouse/Farm

Each expansion:

- Increases capacity
- Allows more setups
- Introduces new systems

---

## Integration with Systems

### 🧠 Needs System

- Low energy reduces ability to manage systems
- Poor needs reduce efficiency
- High needs improve productivity

---

### 💰 Economy System

- Production generates income
- Income fuels upgrades and expansion
- Scaling increases earning potential

---

### 📋 Mission System

- Missions guide:
  - Setup creation
  - Expansion
  - Optimization
- Unlock new systems and spaces

---

### 💬 Social System

- Unlock opportunities (workers, locations)
- Enables scaling beyond solo play

---

## Event Hooks (IMPORTANT)

The Grow System should emit events such as:

- OnProductionStarted
- OnProductionTick
- OnProductionComplete
- OnGrowSetupPlaced
- OnGrowUpgraded
- OnEfficiencyChanged

These events are used by:

- [[Mission_System_Overview]]
- [[Mission_Flow]]
- [[UI_Overview]]
- [[Economy_System]]

---

## Design Goals

- Replace traditional jobs with interactive systems
- Reward player optimization and planning
- Scale complexity over time
- Integrate seamlessly with life simulation systems
- Provide clear and satisfying feedback loops