# Social System

## Links

**Depends On:**
- [[Needs_System]]

**Used By:**
- [[Mission_System_Overview]]
- [[Career_System]]
- [[Progression_System]]
- [[Core_Gameplay_Loop]]

---

## Overview

The Social System governs relationships, interactions, and social progression.

It plays a key role in:

- Unlocking missions and opportunities
- Expanding gameplay options
- Supporting progression alongside the grow system

---

## Relationship Types

- Acquaintance
- Friend
- Close Friend (optional expansion)
- Enemy (optional)

---

## Mechanics

- Relationship levels increase through interaction
- Affected by:
  - Mood
  - Hygiene
  - Social need level
  - Traits (future system)

- Relationships decay over time if neglected

---

## Integration with Systems

### 📋 Mission System

- Many missions require:
  - Building relationships
  - Interacting with specific characters
- Social progress unlocks new mission chains

---

### 🌱 Grow System

- Social connections may unlock:
  - New opportunities
  - Access to new locations or resources
  - Potential worker recruitment

---

### 🧭 Progression System

- Certain progression stages require:
  - Social milestones
  - Relationship thresholds

---

### 🧠 Needs System

- Social need directly affects:
  - Interaction success
  - Relationship growth speed

---

## Gameplay Impact

- Unlock new missions and objectives
- Enable expansion opportunities
- Support hiring workers
- Provide alternative progression paths beyond production

---

## Event Hooks (IMPORTANT)

The Social System should emit events such as:

- OnRelationshipIncreased
- OnRelationshipLevelUp
- OnNewContactUnlocked

These events are used by:

- [[Mission_System_Overview]]
- [[Mission_Flow]]
- [[UI_Overview]]

---

## Design Goals

- Encourage player interaction beyond production systems
- Balance life simulation with operational gameplay
- Provide meaningful progression through relationships
- Integrate naturally with missions and progression systems