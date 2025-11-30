# Lineage of the Arcane - Game Design Document

## Overview

**Lineage of the Arcane** is a gameplay prototype exploring a unique magic system where spells are sentient entities. Players do not cast magic; they negotiate with ancient "Progenitors" (Parents) to lend their aid.

> "Magic is not a tool. It is a family tree. And you are the youngest child."

---

## Core Concept

In this game, magic is not a resource to be spent—it's a relationship to be maintained. Each magical entity (called a "Parent") has its own personality, demands, and risks. Summoning one creates a "Tether" that drains the player's life force.

---

## Core Mechanics

### 1. The Tether System

**Concept:** Mana does not exist. Summoning drains the user's max health (Physical Tether) and sanity.

**How It Works:**
- When a player summons a Parent, a Tether is formed
- The Tether continuously drains the player's current health
- Different Parents have different drain rates
- If health drops to zero while tethered, the Tether snaps

**Risk/Reward:**
- High-power Parents have higher drain rates
- Players must balance power gained vs. health lost
- Strategic timing of tethering is crucial

### 2. Ancestral Temperament

**Concept:** Summons have specific personalities (Aggressive, Passive, Rhythm-based). Failing to adhere to their playstyle results in the summon damaging the player.

**Personality Types:**
| Type | Requirement | Example Entity |
|------|-------------|----------------|
| Aggressive | Must attack frequently | Ignis Mater |
| Passive | Must avoid combat | Aqua Pater |
| Rhythmic | Must follow patterns | Tempus Mater |
| Sacrificial | Must take damage | Dolor Mater |

**Punishment System:**
- Each Parent monitors player behavior
- Violating temperament triggers punishment
- Punishments include damage, debuffs, or instant tether break

### 3. Environmental Shifts

**Concept:** Summoning a Parent changes the physics and lighting of the game map globally.

**Examples:**
- **Ignis Mater:** Ambient light turns red, floor becomes hazardous
- **Aqua Pater:** World becomes flooded, movement slowed
- **Terra Mater:** Gravity increases, structures emerge

---

## Entity Hierarchy

### Tier 0: The Parents (Progenitors)
- Most powerful entities
- Highest tether cost
- Global environmental effects
- Complex temperament requirements

### Tier 1: The Scions
- Descendants of Parents
- Moderate tether cost
- Local environmental effects
- Simpler temperament requirements

### Tier 2: The Heirs
- Weakest magical entities
- Low tether cost
- Minimal environmental effects
- Forgiving temperament

---

## The Rampant State

When a Tether breaks unexpectedly (not manually severed), the Parent enters a **Rampant** state:

- The Parent is no longer bound to the player
- It may attack the player or act chaotically
- Environmental effects remain active
- Player must flee or find a way to rebind

---

## Planned Features

### Custody Battle (Multiplayer)
A competitive mode where two players attempt to tether the same Parent:
- Both players' health drains faster
- The Parent favors whoever better matches its temperament
- One player eventually "wins" the Parent
- Loser suffers backlash damage

### Evolution System
Players can strengthen bonds over time:
- Successful tethering increases affinity
- High affinity reduces tether cost
- Max affinity unlocks special abilities
- Parents may "remember" betrayals

---

## Technical Architecture

### Script Structure
```
Assets/Scripts/
├── Core/
│   ├── MagicParent.cs     - Abstract base class for all entities
│   ├── TetherSystem.cs    - Manages tether logic and health drain
│   └── RampantState.cs    - Handles rampant AI behavior
├── Entities/
│   ├── IgnisMater.cs      - Fire Mother implementation
│   ├── AquaPater.cs       - Water Father implementation
│   ├── TerraMater.cs      - Earth Mother implementation
│   └── Tiers/
│       ├── Scion.cs       - Tier 1 base class
│       ├── Heir.cs        - Tier 2 base class
│       ├── EmberScion.cs  - Fire Scion implementation
│       └── CandlelightHeir.cs - Fire Heir implementation
├── Effects/
│   └── TetherVisualEffect.cs - Line renderer for tether visualization
├── Audio/
│   ├── AudioManager.cs    - Singleton audio management system
│   └── ParentAudioProfile.cs - Audio configuration for Parents
├── UI/
│   ├── HealthBarUI.cs     - Health bar with burned health overlay
│   ├── SanityIndicatorUI.cs - Sanity display with peripheral effects
│   └── TetherDisplayUI.cs - Tether status and temperament indicator
├── Multiplayer/
│   └── CustodyBattle.cs   - Multiplayer tug-of-war system
└── Player/
    └── PlayerController.cs - Player state and combat tracking
```

### Class Relationships
```
MagicParent (Abstract)
    ↳ IgnisMater (Tier 0 - Parent, Aggressive)
    ↳ AquaPater (Tier 0 - Parent, Passive)
    ↳ TerraMater (Tier 0 - Parent, Rhythmic)
    ↳ Scion (Abstract - Tier 1)
        ↳ EmberScion
    ↳ Heir (Abstract - Tier 2)
        ↳ CandlelightHeir
    ↳ (Future) TempusMater

RampantState
    → Attached to MagicParent
    → Manages rampant behavior

TetherSystem
    → References MagicParent
    → References PlayerController

TetherVisualEffect
    → References TetherSystem
    → Renders visual connection

AudioManager (Singleton)
    → Manages all game audio
    → References ParentAudioProfile

CustodyBattle
    → References MagicParent
    → References multiple PlayerControllers

PlayerController
    → Tracked by MagicParent instances

UI Components
    → HealthBarUI → References PlayerController
    → SanityIndicatorUI → References PlayerController
    → TetherDisplayUI → References TetherSystem
```

---

## UI Elements (Planned)

### Health Bar
- Standard red bar for current health
- Grey overlay showing "burned" max health from tethering

### Sanity Indicator
- Peripheral screen effects at low sanity
- Sound distortion effects

### Tether Indicator
- Visual line connecting player to summon
- Color indicates Parent's current temperament status

---

## Audio Design (Planned)

- Each Parent has a unique ambient sound
- Temperament violations trigger warning sounds
- Rampant state has intense audio cues
- Environmental shifts affect ambient soundscape

---

## Art Direction (Planned)

- Dark fantasy aesthetic
- Each Parent has a distinct visual identity
- Tether effects glow with Parent's color
- Environmental shifts are dramatic but readable

---

## Development Roadmap

### Phase 1: Core Mechanics ✅
- [x] Base MagicParent class
- [x] TetherSystem implementation
- [x] PlayerController
- [x] Ignis Mater (first Parent)
- [x] Security scanning (CodeQL)

### Phase 2: Content Expansion ✅
- [x] Additional Parents (Aqua Pater, Terra Mater)
- [x] Tier 1 Scions (Base class + Ember Scion)
- [x] Tier 2 Heirs (Base class + Candlelight Heir)

### Phase 3: Polish ✅
- [x] Visual effects for tethering
- [x] Audio implementation
- [x] UI systems

### Phase 4: Advanced Features ✅
- [x] Custody Battle multiplayer mode
- [ ] Evolution/Affinity system
- [x] Rampant AI behaviors

---

## License

MIT License - See LICENSE file for details.
