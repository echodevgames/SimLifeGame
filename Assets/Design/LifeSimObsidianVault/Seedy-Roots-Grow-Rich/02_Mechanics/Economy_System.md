# Economy System

## Links

**Depends On:**
- [[Grow_System]]
- [[Mission_System_Overview]]

**Used By:**
- [[Unlockables]]
- [[Items]]
- [[Progression_System]]
- [[Core_Gameplay_Loop]]

---

## Overview

The Economy System governs income generation, spending, and progression pacing.

Unlike traditional life sims, income is primarily generated through:

- Player-managed grow operations
- Production efficiency and scaling
- Mission rewards

---

## Income Sources

### 🌱 Production (Primary)

- Income generated over time from grow setups
- Output is influenced by:
  - Equipment quality
  - Layout and efficiency
  - Player interaction
  - Needs state

---

### 📋 Missions

- Rewards for completing objectives
- Used to accelerate progression and unlock systems

---

### 🧩 Side Activities (Optional)

- Social or gameplay-driven activities
- Minor supplemental income or bonuses

---

## Expenses

### 🧱 Expansion

- New rooms, spaces, and properties
- Increasing available grow area

---

### ⚙️ Equipment & Upgrades

- Grow equipment (lights, systems, tools)
- Efficiency upgrades
- Advanced production systems

---

### 🏠 Home & Lifestyle

- Furniture and comfort items
- Social and personal upgrades
- Optional bills or upkeep systems

---

## Economic Loop

Production → Income → Reinvestment → Expansion → Increased Production

This loop drives the entire progression system.

---

## Integration with Systems

### 🌱 Grow System

- Primary driver of income
- Efficiency directly impacts earnings
- Scaling increases output potential

---

### 🧭 Progression System

- Income enables unlocking new stages
- Higher tiers require greater investment

---

### 📋 Mission System

- Missions provide bursts of income and unlock opportunities
- Guide players toward profitable decisions

---

### 🧠 Needs System

- Poor needs reduce efficiency and income
- Well-managed needs improve productivity

---

## Event Hooks (IMPORTANT)

The Economy System should emit events such as:

- OnMoneyEarned
- OnMoneySpent
- OnIncomeThresholdReached
- OnPurchaseCompleted

These events are used by:

- [[Mission_System_Overview]]
- [[Mission_Flow]]
- [[UI_Overview]]

---

## Design Goals

- Make income directly tied to player decisions and system management
- Encourage reinvestment into growth and efficiency
- Maintain meaningful progression pacing
- Prevent runaway income without scaling complexity
- Reinforce the connection between life management and production success