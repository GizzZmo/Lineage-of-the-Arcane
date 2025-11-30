# Project Structure

This document describes the organization of the Lineage of the Arcane codebase.

## Directory Overview

```
Lineage-of-the-Arcane/
├── .github/
│   └── workflows/           # CI/CD pipeline configurations
│       ├── unity-ci.yml     # Unity build and test workflow
│       └── security.yml     # Security scanning workflow
├── Assets/
│   ├── Docs/
│   │   └── GDD.md           # Game Design Document
│   └── Scripts/             # All C# game scripts
│       ├── Core/            # Core game systems
│       ├── Entities/        # Magic entity implementations
│       ├── Effects/         # Visual effects
│       ├── Audio/           # Audio management
│       ├── UI/              # User interface components
│       ├── Multiplayer/     # Multiplayer systems
│       └── Player/          # Player-related scripts
├── wiki/                    # Documentation wiki pages
├── README.md                # Project overview
├── LICENSE                  # MIT License
└── .gitignore               # Git ignore rules
```

## Scripts Directory Structure

### Core (`Assets/Scripts/Core/`)

The foundational systems that power the game mechanics.

| File | Purpose | Dependencies |
|------|---------|--------------|
| `MagicParent.cs` | Abstract base class for all magical entities | RampantState, AffinitySystem |
| `TetherSystem.cs` | Manages tether connections and health drain | MagicParent, PlayerController, AffinitySystem |
| `RampantState.cs` | Handles AI behavior when tethers break | MagicParent, PlayerController |
| `AffinitySystem.cs` | Singleton tracking player-entity relationships | None (standalone singleton) |

### Entities (`Assets/Scripts/Entities/`)

Implementations of specific magical beings.

| File | Purpose | Base Class |
|------|---------|------------|
| `IgnisMater.cs` | Fire Mother - Aggressive temperament | MagicParent |
| `AquaPater.cs` | Water Father - Passive temperament | MagicParent |
| `TerraMater.cs` | Earth Mother - Rhythmic temperament | MagicParent |

#### Tiers (`Assets/Scripts/Entities/Tiers/`)

Lower-tier magical entities with simpler requirements.

| File | Purpose | Base Class |
|------|---------|------------|
| `Scion.cs` | Abstract base for Tier 1 entities | MagicParent |
| `Heir.cs` | Abstract base for Tier 2 entities | MagicParent |
| `EmberScion.cs` | Fire Scion implementation | Scion |
| `CandlelightHeir.cs` | Fire Heir implementation | Heir |

### Effects (`Assets/Scripts/Effects/`)

Visual effect systems.

| File | Purpose | Dependencies |
|------|---------|--------------|
| `TetherVisualEffect.cs` | Line renderer for tether visualization | TetherSystem, LineRenderer |

### Audio (`Assets/Scripts/Audio/`)

Sound management and configuration.

| File | Purpose | Pattern |
|------|---------|---------|
| `AudioManager.cs` | Singleton audio management | Singleton |
| `ParentAudioProfile.cs` | ScriptableObject for Parent audio config | ScriptableObject |

### UI (`Assets/Scripts/UI/`)

User interface components.

| File | Purpose | Dependencies |
|------|---------|--------------|
| `HealthBarUI.cs` | Health bar with burned health overlay | PlayerController |
| `SanityIndicatorUI.cs` | Sanity display with peripheral effects | PlayerController, AudioManager |
| `TetherDisplayUI.cs` | Tether status and temperament indicator | TetherSystem |
| `AffinityDisplayUI.cs` | Affinity level and progress display | AffinitySystem, TetherSystem |

### Multiplayer (`Assets/Scripts/Multiplayer/`)

Multiplayer game modes.

| File | Purpose | Dependencies |
|------|---------|--------------|
| `CustodyBattle.cs` | Two-player tug-of-war system | MagicParent, PlayerController, TetherSystem |

### Player (`Assets/Scripts/Player/`)

Player-related functionality.

| File | Purpose | Dependencies |
|------|---------|--------------|
| `PlayerController.cs` | Player state, health, combat tracking | Rigidbody |

## Class Hierarchy

```
MonoBehaviour
├── MagicParent (abstract)
│   ├── IgnisMater
│   ├── AquaPater
│   ├── TerraMater
│   ├── Scion (abstract)
│   │   └── EmberScion
│   └── Heir (abstract)
│       └── CandlelightHeir
├── TetherSystem
├── RampantState
├── AffinitySystem (Singleton)
├── AudioManager (Singleton)
├── PlayerController
├── CustodyBattle
├── TetherVisualEffect
├── HealthBarUI
├── SanityIndicatorUI
├── TetherDisplayUI
└── AffinityDisplayUI

ScriptableObject
└── ParentAudioProfile
```

## Dependency Graph

```
                    ┌─────────────────┐
                    │  AffinitySystem │
                    │   (Singleton)   │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
       ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
       │ MagicParent │ │TetherSystem │ │AffinityDisp.│
       │  (abstract) │ │             │ │     UI      │
       └──────┬──────┘ └──────┬──────┘ └─────────────┘
              │               │
              ├───────────────┤
              │               │
              ▼               ▼
       ┌─────────────┐ ┌─────────────┐
       │ RampantState│ │PlayerControl│
       │             │ │             │
       └─────────────┘ └──────┬──────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
       ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
       │ HealthBarUI │ │SanityIndic. │ │TetherDisp.  │
       │             │ │     UI      │ │     UI      │
       └─────────────┘ └─────────────┘ └─────────────┘
```

## Design Patterns Used

### Singleton Pattern
- `AffinitySystem` - Global relationship tracking
- `AudioManager` - Global audio management

### Template Method Pattern
- `MagicParent.CheckTemperament()` - Abstract method implemented by each entity
- `MagicParent.ApplyEnvironmentalShift()` - Abstract method for environmental effects

### Observer Pattern
- `AffinitySystem` events (`OnAffinityLevelChanged`, `OnAffinityGained`, etc.)
- UI components subscribe to these events for updates

### Component Pattern
- `MagicParent` requires `RampantState` via `RequireComponent`
- Systems are attached as components to GameObjects

## File Naming Conventions

| Convention | Example | Used For |
|------------|---------|----------|
| PascalCase | `TetherSystem.cs` | All C# script files |
| Suffix: `UI` | `HealthBarUI.cs` | UI component scripts |
| Suffix: `System` | `AffinitySystem.cs` | Core system managers |
| Suffix: `Profile` | `ParentAudioProfile.cs` | ScriptableObject configs |
| Suffix: `Controller` | `PlayerController.cs` | Entity controllers |

## Adding New Files

### Adding a New Parent Entity

1. Create file in `Assets/Scripts/Entities/`
2. Inherit from `MagicParent`
3. Implement required abstract methods:
   - `ApplyEnvironmentalShift()`
   - `CheckTemperament()`
4. Configure in `Start()`:
   - Set `entityName`, `entityId`
   - Configure rampant behavior
5. Document in wiki under [[Parents]]

### Adding a New Scion

1. Create file in `Assets/Scripts/Entities/Tiers/`
2. Inherit from `Scion`
3. Implement:
   - `ApplyLocalEnvironmentalShift()`
   - `PerformTemperamentCheck()`
4. Set `parentLineage` to connect to parent entity
5. Document in wiki under [[Scions]]

### Adding a New Heir

1. Create file in `Assets/Scripts/Entities/Tiers/`
2. Inherit from `Heir`
3. Implement:
   - `ApplyMinimalEnvironmentalShift()`
   - `PerformTemperamentCheck()`
4. Set `ancestralLine` to connect to lineage
5. Document in wiki under [[Heirs]]

---

[← Back to Home](Home) | [Getting Started →](Getting-Started)
