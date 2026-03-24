# Mission Flow

## Links

**Depends On:**
- [[Mission_System_Overview]]
- [[Core_Gameplay_Loop]]

**Used By:**
- [[Reward_System]]
- [[Mission_UI]]
- [[Progression_System]]

---

## Lifecycle

1. Mission Assigned
2. Player Performs Actions
3. System Events Trigger Progress
4. Progress Tracked
5. Completion Triggered
6. Reward Granted
7. New Mission or Unlock Activated

---

## Core Flow (Detailed)

### 1. Mission Assigned

- Player receives a new mission
- Mission appears in [[Mission_UI]]
- Objectives are clearly defined

---

### 2. Player Performs Actions

Actions can come from multiple systems:

- Life Simulation:
  - Social interactions
  - Needs management
  - Home upgrades

- Grow System:
  - Setting up equipment
  - Maintaining production
  - Expanding grow space

---

### 3. System Events Trigger Progress

Game systems emit events such as:

- OnMoneyEarned
- OnProductionComplete
- OnGrowUpgraded
- OnRelationshipIncreased
- OnItemPurchased

These events are used to update mission progress.

---

### 4. Progress Tracked

- MissionManager listens for relevant events
- Progress updates in real-time
- UI reflects current progress

---

### 5. Completion Triggered

- All objectives are met
- Mission is marked complete
- Completion feedback is shown

---

### 6. Reward Granted

Rewards may include:

- Money
- Unlocks (equipment, locations, systems)
- New mission chains
- Progression advancement

---

### 7. New Mission or Unlock Activated

- New missions become available
- New systems or areas unlock
- Player progression continues

---

## Flow Diagram (Conceptual)

Player Action → System Event → Mission Progress → Completion → Reward → Unlock → Next Mission

---

## Design Notes

- Multiple missions active simultaneously
- Missions should progress naturally through gameplay
- Player actions should feel directly tied to progress
- Clear feedback at every stage
- Both life simulation and grow systems contribute to mission completion