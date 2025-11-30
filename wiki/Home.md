# Lineage of the Arcane Wiki

> *"Magic is not a tool. It is a family tree. And you are the youngest child."*

Welcome to the official documentation wiki for **Lineage of the Arcane: The Parents of Magic** - a Unity game prototype exploring a revolutionary magic system where spells are sentient entities that players must negotiate with rather than simply cast.

---

## 📚 Documentation Index

### Getting Started
- [[Getting-Started|Getting Started Guide]] - How to set up and run the project
- [[Project-Structure|Project Structure]] - Understanding the codebase organization
- [[Core-Concepts|Core Concepts]] - Key gameplay mechanics and terminology

### Core Systems
- [[Tether-System|Tether System]] - The health-drain magic binding mechanic
- [[Affinity-System|Affinity System]] - Evolution and relationship tracking
- [[Rampant-System|Rampant System]] - AI behavior when tethers break
- [[Temperament-System|Temperament System]] - Parent personality requirements

### Entities
- [[Magic-Parent|Magic Parent Base Class]] - The foundation of all magical entities
- [[Parents|Parent Entities (Tier 0)]] - Ignis Mater, Aqua Pater, Terra Mater
- [[Scions|Scion Entities (Tier 1)]] - Mid-tier magical descendants
- [[Heirs|Heir Entities (Tier 2)]] - Gentle, forgiving magical beings

### Game Systems
- [[Player-Controller|Player Controller]] - Player state and combat
- [[Custody-Battle|Custody Battle]] - Multiplayer tug-of-war mechanics
- [[Audio-System|Audio System]] - Sound management and profiles
- [[Visual-Effects|Visual Effects]] - Tether visualization and effects

### UI Components
- [[UI-Overview|UI Overview]] - All user interface components
- [[Health-Bar-UI|Health Bar]] - Health display with burned health
- [[Sanity-Indicator-UI|Sanity Indicator]] - Sanity effects and display
- [[Tether-Display-UI|Tether Display]] - Connection status UI
- [[Affinity-Display-UI|Affinity Display]] - Relationship progress UI

### API Reference
- [[API-Reference|Complete API Reference]] - All public methods and properties
- [[Events|Events and Callbacks]] - System events and hooks
- [[Scriptable-Objects|Scriptable Objects]] - Configuration assets

---

## 🎮 Quick Overview

### What Makes This Magic System Unique?

In **Lineage of the Arcane**, magic isn't a resource to be spent—it's a relationship to be maintained. Each magical entity (called a "Parent") has its own:

- **Personality** (Temperament) - Behavioral requirements you must satisfy
- **Cost** (Tether Drain) - Continuous health drain while connected
- **Memory** (Affinity) - They remember how you've treated them

### The Three Core Mechanics

| Mechanic | Description |
|----------|-------------|
| **Tether System** | Form magical bonds that drain your life force in exchange for power |
| **Temperament System** | Each entity has personality requirements - violate them at your peril |
| **Affinity System** | Build relationships over time for reduced costs and special abilities |

### Entity Hierarchy

```
Tier 0: Parents (Progenitors)
├── Ignis Mater - The Fire Mother (Aggressive)
├── Aqua Pater - The Water Father (Passive)
└── Terra Mater - The Earth Mother (Rhythmic)

Tier 1: Scions (Descendants)
└── Ember Scion - Fire descendant

Tier 2: Heirs (Weakest)
└── Candlelight Heir - Gentle fire heir
```

---

## 🔧 Technical Requirements

- **Unity Version**: 2021.3 LTS or later
- **Platform**: .NET Standard 2.1
- **Dependencies**: None (vanilla Unity)

---

## 📖 Further Reading

- [Game Design Document](../Assets/Docs/GDD.md) - Complete design specifications
- [README](../README.md) - Project overview and setup
- [LICENSE](../LICENSE) - MIT License

---

## 🤝 Contributing

This project follows standard Unity development practices. When contributing:

1. Follow the existing code style and comment conventions
2. Ensure all new public methods have XML documentation
3. Update relevant wiki pages for new features
4. Add appropriate unit tests where applicable

---

*"The blood you offer is not payment. It is a handshake."*
