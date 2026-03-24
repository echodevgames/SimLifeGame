# Unity Implementation

## Links

**Depends On:**
- [[Technical_Architecture]]

**Used By:**
- (Implementation Layer)

---

## Engine

Unity 6 (6000.x)

---

## Core Implementation Strategy

The game is built using a hybrid approach:

- MonoBehaviours for runtime systems
- ScriptableObjects for data-driven design
- Event-driven communication between systems

---

## Key Systems Mapping

### 📋 Mission System

MissionManager → MonoBehaviour  
MissionData → ScriptableObject  
ActiveMission → Runtime class

---

### 🌱 Grow System

GrowManager → MonoBehaviour  
GrowSetup → MonoBehaviour  
GrowData → ScriptableObject (optional future)

---

### 🔓 Unlock System

UnlockManager → MonoBehaviour  
UnlockData → ScriptableObject

---

### 💰 Economy System

EconomyManager → MonoBehaviour

---

### 🧠 Needs System

NeedsManager → MonoBehaviour

---

### 💬 Social System

SocialManager → MonoBehaviour

---

### 🧭 Game State

GameStateManager → Singleton MonoBehaviour

---

## ScriptableObject Usage

Used for:

- Missions
- Items
- Unlock data
- Grow data (future expansion)

---

## Event System

Core gameplay is driven by events.

### Example Flow

Player earns money  
→ OnMoneyEarned event fired  
→ MissionManager updates progress  
→ Mission completes  
→ UnlockManager unlocks content  
→ UI updates  

---

### Example Event Types

- OnMoneyEarned
- OnProductionComplete
- OnGrowSetupPlaced
- OnNeedChanged
- OnRelationshipLevelUp

---

## UI Implementation

- Canvas-based UI system
- TextMeshPro for text rendering
- Event-driven updates for UI elements

---

## Design Goals

- Keep systems modular and reusable
- Minimize direct dependencies between systems
- Use events to drive gameplay logic
- Support scalability and future expansion