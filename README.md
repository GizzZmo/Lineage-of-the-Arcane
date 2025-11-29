# Lineage of the Arcane: The Parents of Magic

> "Magic is not a tool. It is a family tree. And you are the youngest child."

![CI/CD](https://github.com/GizzZmo/Lineage-of-the-Arcane-Unity-C-/actions/workflows/unity-ci.yml/badge.svg)

## 🔮 Project Overview

**Lineage of the Arcane** is a gameplay prototype exploring a unique magic system where spells are sentient entities. Players do not cast magic; they negotiate with ancient "Progenitors" (Parents) to lend their aid.

This Unity project implements a revolutionary magic system where power comes at a cost - your very life force.

## ⚙️ Core Mechanics Implemented

### 1. The Tether System
Mana does not exist. Summoning drains the user's max health (Physical Tether) and sanity.
- **Health Drain**: Continuous cost while tethered
- **Risk/Reward**: High power = High drain rate
- **Tether Break**: When health is depleted, the bond snaps violently

### 2. Ancestral Temperament
Summons have specific personalities (Aggressive, Passive, Rhythm-based). Failing to adhere to their playstyle results in the summon damaging the player.
- **Ignis Mater**: Demands constant aggression
- **Punishment System**: Violate temperament = take damage

### 3. Environmental Shifts
Summoning a Parent changes the physics and lighting of the game map globally.
- Each Parent has unique visual effects
- World-altering presence

## 📂 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── MagicParent.cs      # Abstract base for all Parents
│   │   └── TetherSystem.cs     # Health-drain mechanic
│   ├── Entities/
│   │   └── IgnisMater.cs       # Fire Mother implementation
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
| `IgnisMater.cs` | "Fire Mother" - aggressive temperament entity |
| `PlayerController.cs` | Player health, combat, and movement |

## 🚀 Roadmap

- [x] Core Tether System
- [x] Base Parent class hierarchy
- [x] First entity: Ignis Mater
- [x] Player controller with combat tracking
- [x] CI/CD Pipeline with artifacts
- [ ] Implement "Custody Battle" (Multiplayer tug-of-war)
- [ ] Add Tier 1 (Scions) and Tier 2 (Heirs)
- [ ] Create "Rampant" AI state when Tether breaks
- [ ] Visual effects for tethering
- [ ] Audio implementation

## 🔄 CI/CD Pipeline

This project includes a GitHub Actions workflow that:
- ✅ Validates C# code syntax
- 📦 Packages scripts and documentation as artifacts
- 📊 Generates code analysis reports
- 🏷️ Creates releases automatically on version tags

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
