# Unity 6 Implementation Plan

## Links

**Depends On:**
- [[Technical Architecture]]

**Used By:**
- (Implementation Layer)

---

## Engine
Unity 6 (6000.x)

## Key Systems Mapping

MissionManager → MonoBehaviour + ScriptableObjects
- MissionData (SO)
- ActiveMission instances

GameStateManager → Singleton

UnlockManager → Data-driven (ScriptableObjects)

UI System
- TextMeshPro
- Canvas-based HUD

## Suggested Patterns

- ScriptableObjects for:
  - Missions
  - Items
  - Careers

- Event-driven architecture:
  - OnMoneyEarned
  - OnSkillLevelUp
  - OnMissionCompleted

## Example Flow

Player earns money →
Event fired →
MissionManager updates progress →
Mission completes →
UnlockManager unlocks content →
UI updates