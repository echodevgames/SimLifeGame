# Technical Architecture

## Links

**Depends On:**
- [[Progression_System]]
- [[Mission_System_Overview]]

**Used By:**
- [[Unity_Implementation]]

---

## Core Managers

### GameStateManager
- Tracks progression stage
- Handles game mode (Story / Free)

### MissionManager
- Tracks active missions
- Handles completion
- Issues rewards

### UnlockManager
- Controls locked/unlocked content

### SimController
- Manages needs
- Handles player state

### EconomyManager
- Tracks money
- Handles transactions

## Data Flow

Player Action → System Update → Mission Progress → Reward → Unlock → Expanded Gameplay