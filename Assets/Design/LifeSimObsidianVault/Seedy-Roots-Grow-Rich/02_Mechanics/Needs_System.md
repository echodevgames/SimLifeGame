# Needs System

## Links

**Depends On:**
- [[Core_Gameplay_Loop]]

**Used By:**
- [[Career_System]]
- [[Social_System]]
- [[Mission_System_Overview]]
- [[Needs_UI]]
- [[Grow_System]]
- [[Economy_System]]

---

## Overview

The Needs System drives player behavior, time management, and decision-making.

It creates constant pressure that forces the player to balance:

- Personal well-being
- Grow operation management
- Social and progression activities

---

## Core Needs

- Hunger
- Energy
- Hygiene
- Social
- Fun (optional expansion)

---

## Mechanics

- Needs decay over time
- Low needs negatively affect efficiency and productivity
- High needs provide bonuses to performance and interaction

---

## Gameplay Impact

| Need | Effect |
|------|--------|
| Hunger | Reduces energy recovery and overall efficiency |
| Energy | Limits ability to maintain and interact with grow systems |
| Hygiene | Affects social interactions and opportunities |
| Social | Impacts mood and relationship progression |

---

## Integration with Systems

### 🌱 Grow System

- Low Energy reduces ability to manage grow setups effectively
- Poor needs can reduce production efficiency
- Neglected needs can slow or disrupt progression

---

### 💰 Economy System

- Poor needs → reduced efficiency → lower income
- Good needs → better productivity → higher output

---

### 📋 Mission System

- Certain missions may require:
  - Maintaining needs above thresholds
  - Performing actions while in good condition
- Needs indirectly affect mission completion speed

---

### 💬 Social System

- Hygiene and Social needs directly impact:
  - Relationship building
  - Social opportunities
  - Mission availability

---

## Event Hooks (IMPORTANT)

The Needs System should emit events such as:

- OnNeedLow
- OnNeedCritical
- OnNeedRecovered
- OnAllNeedsStable

These events can be used by:

- [[Mission_System_Overview]]
- [[Mission_Flow]]
- [[UI_Overview]]

---

## Design Goals

- Create constant pressure on the player
- Force meaningful trade-offs between life and production
- Tie directly into all major systems
- Reinforce the balance between personal needs and operational success