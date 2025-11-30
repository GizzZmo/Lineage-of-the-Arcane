# Lineage of the Arcane: The Parents of Magic

> "Magic is not a tool. It is a family tree. And you are the youngest child."

![CI/CD](https://github.com/GizzZmo/Lineage-of-the-Arcane-Unity-C-/actions/workflows/unity-ci.yml/badge.svg)
![Security](https://github.com/GizzZmo/Lineage-of-the-Arcane-Unity-C-/actions/workflows/security.yml/badge.svg)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

## 🔮 Project Overview

**Lineage of the Arcane** is a gameplay prototype exploring a unique magic system where spells are sentient entities. Players do not cast magic; they negotiate with ancient "Progenitors" (Parents) to lend their aid.

This Unity project implements a revolutionary magic system where power comes at a cost - your very life force.

## 📚 Documentation

Comprehensive documentation is available in the [Wiki](wiki/Home.md):

- **[Getting Started](wiki/Getting-Started.md)** - Setup and first steps
- **[Core Concepts](wiki/Core-Concepts.md)** - Understanding the magic system
- **[Project Structure](wiki/Project-Structure.md)** - Codebase organization
- **[API Reference](wiki/API-Reference.md)** - Complete method documentation

### System Documentation
- [Tether System](wiki/Tether-System.md) - Health-drain magic binding
- [Affinity System](wiki/Affinity-System.md) - Relationship tracking
- [Rampant System](wiki/Rampant-System.md) - Hostile AI behavior
- [Temperament System](wiki/Temperament-System.md) - Entity requirements

### Entity Documentation
- [Magic Parent Base Class](wiki/Magic-Parent.md) - Entity foundation
- [Parents (Tier 0)](wiki/Parents.md) - Ignis Mater, Aqua Pater, Terra Mater
- [Scions (Tier 1)](wiki/Scions.md) - Mid-tier descendants
- [Heirs (Tier 2)](wiki/Heirs.md) - Gentle, forgiving entities

## ⚙️ Core Mechanics Implemented

### 1. The Tether System
Mana does not exist. Summoning drains the user's max health (Physical Tether) and sanity.
- **Health Drain**: Continuous cost while tethered
- **Risk/Reward**: High power = High drain rate
- **Tether Break**: When health is depleted, the bond snaps violently

### 2. Ancestral Temperament
Summons have specific personalities (Aggressive, Passive, Rhythm-based). Failing to adhere to their playstyle results in the summon damaging the player.
- **Ignis Mater**: Demands constant aggression
- **Aqua Pater**: Demands passivity - no combat
- **Terra Mater**: Demands rhythmic, patterned actions
- **Punishment System**: Violate temperament = take damage

### 3. Environmental Shifts
Summoning a Parent changes the physics and lighting of the game map globally.
- Each Parent has unique visual effects
- World-altering presence

### 4. Evolution/Affinity System
Build relationships with magical entities over time:
- **Affinity Levels**: Progress from Stranger → Acquainted → Bonded → Devoted → Ascended
- **Reduced Costs**: Higher affinity means lower tether drain (up to 50% reduction)
- **Special Abilities**: Reach Ascended level to unlock unique powers
- **Betrayal Memory**: Entities remember broken tethers
- **Cross-Lineage Bonding**: Training with Heirs and Scions builds affinity with their Parent

## 📂 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── MagicParent.cs      # Abstract base for all Parents
│   │   ├── TetherSystem.cs     # Health-drain mechanic
│   │   ├── RampantState.cs     # Rampant AI behavior system
│   │   └── AffinitySystem.cs   # Evolution/Affinity tracking
│   ├── Entities/
│   │   ├── IgnisMater.cs       # Fire Mother implementation
│   │   ├── AquaPater.cs        # Water Father implementation
│   │   ├── TerraMater.cs       # Earth Mother implementation
│   │   └── Tiers/
│   │       ├── Scion.cs        # Tier 1 base class
│   │       ├── Heir.cs         # Tier 2 base class
│   │       ├── EmberScion.cs   # Fire Scion implementation
│   │       └── CandlelightHeir.cs  # Fire Heir implementation
│   ├── Effects/
│   │   └── TetherVisualEffect.cs   # Tether line renderer effects
│   ├── Audio/
│   │   ├── AudioManager.cs     # Sound management system
│   │   └── ParentAudioProfile.cs   # Audio configuration for Parents
│   ├── UI/
│   │   ├── HealthBarUI.cs      # Health bar display
│   │   ├── SanityIndicatorUI.cs    # Sanity indicator display
│   │   ├── TetherDisplayUI.cs  # Tether status display
│   │   └── AffinityDisplayUI.cs    # Affinity level display
│   ├── Multiplayer/
│   │   └── CustodyBattle.cs    # Multiplayer tug-of-war system
│   └── Player/
│       └── PlayerController.cs  # Player state and combat
└── Docs/
    └── GDD.md                   # Game Design Document
```

## 🛠️ Technical Details

### Dependencies
- Unity 2021.3 LTS or later
- .NET Standard 2.1

### Scripts Overview

| Script | Purpose |
|--------|---------|
| `MagicParent.cs` | Abstract base class defining Parent entity behavior |
| `TetherSystem.cs` | Manages health drain and tether connections |
| `RampantState.cs` | Handles rampant AI behavior when tether breaks |
| `AffinitySystem.cs` | Tracks player-entity relationships and provides benefits |
| `Scion.cs` | Base class for Tier 1 entities |
| `Heir.cs` | Base class for Tier 2 entities |
| `IgnisMater.cs` | "Fire Mother" - aggressive temperament entity |
| `AquaPater.cs` | "Water Father" - passive temperament entity |
| `TerraMater.cs` | "Earth Mother" - rhythmic temperament entity |
| `EmberScion.cs` | Tier 1 fire scion implementation |
| `CandlelightHeir.cs` | Tier 2 fire heir implementation |
| `CustodyBattle.cs` | Multiplayer tug-of-war battle system |
| `PlayerController.cs` | Player health, combat, and movement |
| `TetherVisualEffect.cs` | Line renderer for tether visualization |
| `AudioManager.cs` | Singleton audio management system |
| `ParentAudioProfile.cs` | ScriptableObject for Parent audio configuration |
| `HealthBarUI.cs` | Health bar with burned health overlay |
| `SanityIndicatorUI.cs` | Sanity display with peripheral effects |
| `TetherDisplayUI.cs` | Tether status and temperament indicator |
| `AffinityDisplayUI.cs` | Affinity level and progress display |

## 🌟 Affinity System Details

### Affinity Levels & Benefits

| Level | Requirement | Tether Cost | Special |
|-------|-------------|-------------|---------|
| Hostile | 3+ betrayals | +50% | Entity remembers your treachery |
| Stranger | Default | Normal | No relationship |
| Acquainted | 20% affinity | -10% | Recognition |
| Bonded | 40% affinity | -20% | Trust established |
| Devoted | 70% affinity | -35% | Deep connection |
| Ascended | 100% affinity | -50% | Special ability unlocked |

### Special Abilities (Ascended)

| Entity | Ability | Effect |
|--------|---------|--------|
| Ignis Mater | Inferno Embrace | 5s of invulnerability + AoE damage |
| Aqua Pater | Tidal Sanctuary | 8s healing zone (5 HP/sec) |
| Terra Mater | Earthen Bulwark | Absorbs 50 damage for 10s |

## 🚀 Roadmap

- [x] Core Tether System
- [x] Base Parent class hierarchy
- [x] First entity: Ignis Mater
- [x] Player controller with combat tracking
- [x] CI/CD Pipeline with artifacts
- [x] Security scanning with CodeQL
- [x] Implement "Custody Battle" (Multiplayer tug-of-war)
- [x] Add Tier 1 (Scions) and Tier 2 (Heirs)
- [x] Create "Rampant" AI state when Tether breaks
- [x] Visual effects for tethering
- [x] Audio implementation
- [x] Additional Parents (Aqua Pater, Terra Mater)
- [x] UI systems (Health bar, Sanity indicator, Tether display)
- [x] Evolution/Affinity system with special abilities

## 🔄 CI/CD Pipeline

This project includes a GitHub Actions workflow that:
- ✅ Validates C# code syntax
- 📦 Packages scripts and documentation as artifacts
- 📊 Generates code analysis reports
- 🏷️ Creates releases automatically on version tags

## 🔒 Security

This project includes automated security scanning:
- 🔍 **CodeQL Analysis**: Static analysis for C# vulnerabilities
- 📋 **Dependency Review**: Checks for vulnerable dependencies on PRs
- 🛡️ **Security Hardening**: Validates workflow security practices
- 🔐 **Secret Scanning**: Detects potential hardcoded secrets

### Artifacts Generated
- `scripts-package`: Core game scripts archive
- `docs-package`: Documentation and design docs
- `lineage-of-arcane-full`: Complete project package
- `code-analysis-report`: Static analysis results

## 📖 Documentation

See the [Game Design Document](Assets/Docs/GDD.md) for detailed mechanics, lore, and technical specifications.

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details.

---

*"The blood you offer is not payment. It is a handshake."*
