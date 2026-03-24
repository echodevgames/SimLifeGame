# UI Color Palette & Design System

## Visual Identity

The game moves through two distinct feelings across its arc:

- **Early game** — grimy, DIY, underground grow-room. UI feels rough and functional.
- **Late game** — scaled, optimized, profitable. UI evolves into something cleaner and data-driven.

Every UI decision should reinforce this arc. Colors mean things — never break the meaning.

---

## Color Roles

| Role | Purpose |
|---|---|
| Base Background | UI panels, menus, overlays |
| Primary Accent | Main highlight — buttons, selection, active states |
| Secondary Accent | Supporting highlights — hover states, borders |
| Success | Money, growth, positive feedback |
| Warning | Risk, heat, problems |
| Text Primary | Readable body and label text |
| Text Secondary | Subdued labels, metadata |

---

## Palette

### Base (Dark UI Foundation)

| Name | Hex | Use |
|---|---|---|
| Charcoal Black | `#121212` | Panel backgrounds, overlays |
| Dirty Dark Green | `#1B2A1F` | Inner panel tints, header backgrounds |

### Primary Accent

| Name | Hex | Use |
|---|---|---|
| Muted Plant Green | `#4CAF50` | Buttons, selected states, progress indicators |

### Secondary Accent

| Name | Hex | Use |
|---|---|---|
| Olive / Moss Green | `#6B8E23` | Secondary UI elements, hover states, borders |

### Success

| Name | Hex | Use |
|---|---|---|
| Money Green | `#00C853` | Income ticks, profit UI, "+$" feedback popups |

### Warning / Risk

| Name | Hex | Use |
|---|---|---|
| Amber | `#FFB300` | Caution states, resource warnings |
| Soft Red | `#E53935` | Errors, heat/risk mechanics |

### Text

| Name | Hex | Use |
|---|---|---|
| Primary Text | `#E0E0E0` | All main readable content |
| Secondary Text | `#9E9E9E` | Supporting labels, timestamps, metadata |

---

## Consistency Rules

### Rule 1 — One Primary Color

Green is the interaction and economy color. Do not mix it arbitrarily with blue, purple, or other hues. Every new color added must have an explicit role.

### Rule 2 — Color = Meaning (Never Break This)

| Color | Always Means |
|---|---|
| Green | Good / active / money / growth |
| Red | Bad / error / loss |
| Yellow/Amber | Caution / risk / attention |

If a color is used outside its role, the UI becomes unreadable at a glance.

### Rule 3 — Contrast Over Brightness

Dark panels with selective bright accents = clean look. Do not make everything glow or saturate. Reserve bright accents (`#00C853`, `#4CAF50`) for moments that matter.

---

## UI Style by Game Stage

### Early Game
- Rougher panels, heavier darks
- Minimal visual polish — function over form
- `#1B2A1F` backgrounds, muted greens only

### Late Game (optional evolution)
- Cleaner layout, brighter accents
- More data-dense screens (production stats, yield tracking)
- `#00C853` success feedback becomes more prominent

---

## Quick Reference

```
Charcoal Black    #121212   — background
Dirty Dark Green  #1B2A1F   — panel tint
Muted Green       #4CAF50   — primary accent
Olive Green       #6B8E23   — secondary accent
Money Green       #00C853   — success / income
Amber             #FFB300   — warning
Soft Red          #E53935   — error / risk
Text Primary      #E0E0E0
Text Secondary    #9E9E9E
```

---

*See also: [[UI_Overview]], [[Production_UI]], [[Mission_UI]], [[Needs_UI]]*
