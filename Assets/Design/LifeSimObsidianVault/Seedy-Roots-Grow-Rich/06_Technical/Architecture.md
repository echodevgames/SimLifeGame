# Technical Architecture

## Links

**Depends On:**
- [[Progression_System]]
- [[Mission_System_Overview]]
- [[Grow_System]]

**Used By:**
- [[Unity_Implementation]]

---

## Overview

The architecture is built around modular, event-driven systems.

Each major gameplay system is managed independently, while communicating through events to maintain loose coupling.

---

## Core Managers

### 🧭 GameStateManager

- Tracks progression stage
- Handles game mode (Story / Free)
- Controls global game state

---

### 📋 MissionManager

- Tracks active missions
- Listens for gameplay events
- Updates mission progress
- Handles mission completion and rewards

---

### 🔓 UnlockManager

- Controls locked/unlocked content
- Responds to mission completion and progression
- Enables access to items, locations, and systems

---

### 🌱 GrowManager (CORE SYSTEM)

- Manages all grow setups
- Handles production cycles
- Calculates efficiency and output
- Emits production-related events

---

### 🧠 NeedsManager

- Tracks and updates player needs
- Applies modifiers to gameplay systems
- Emits need-related events

---

### 💰 EconomyManager

- Tracks player money
- Handles transactions (earn/spend)
- Emits economy-related events

---

### 💬 SocialManager

- Tracks relationships and interactions
- Emits social-related events
- Unlocks opportunities and progression paths

---

## Data Flow

Player Action  
→ System Event Triggered  
→ MissionManager Updates Progress  
→ Mission Completed  
→ Reward Granted  
→ UnlockManager Unlocks Content  
→ Systems Expand (Grow / Economy / Locations)  
→ Gameplay Loop Continues

---

## Event-Driven Architecture (IMPORTANT)

All systems communicate via events.

### Example Events

- OnMoneyEarned
- OnProductionComplete
- OnGrowUpgraded
- OnNeedLow
- OnRelationshipLevelUp
- OnItemPurchased

---

## Design Goals

- Keep systems modular and independent
- Use events to connect systems
- Make systems scalable and maintainable
- Ensure clear flow from player action → progression