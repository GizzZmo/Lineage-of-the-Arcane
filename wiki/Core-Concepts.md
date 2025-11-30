# Core Concepts

This document explains the fundamental concepts and terminology used in Lineage of the Arcane.

## The Philosophy

In most games, magic is a resource: you have mana, you spend mana, you cast spells. **Lineage of the Arcane** takes a radically different approach:

> *Magic is not a tool. It is a family tree. And you are the youngest child.*

In this world, magical spells are sentient beings with their own personalities, demands, and memories. You don't "cast" them—you **negotiate** with them.

## Core Terminology

### Entity Types

| Term | Description | Power Level |
|------|-------------|-------------|
| **Parent (Progenitor)** | Ancient, powerful magical beings at the top of their lineage | Tier 0 (Highest) |
| **Scion** | Direct descendants of Parents, moderate power | Tier 1 (Medium) |
| **Heir** | The youngest, gentlest magical beings | Tier 2 (Lowest) |

### Relationship Terms

| Term | Description |
|------|-------------|
| **Tether** | The magical bond between player and entity |
| **Affinity** | The strength of your relationship with an entity |
| **Temperament** | An entity's behavioral requirements |
| **Betrayal** | Breaking a tether unexpectedly (health depleted) |
| **Rampant** | Hostile state when a Parent's tether breaks |

### Mechanical Terms

| Term | Description |
|------|-------------|
| **Tether Cost** | Health drained per second while tethered |
| **Environmental Shift** | World changes caused by an entity's presence |
| **Clean Sever** | Manually ending a tether safely |
| **Lineage Bonus** | Affinity gained with parent entities from their descendants |

## The Three Pillars

### 1. Tether System

The **Tether** is the core mechanic. When you summon an entity:

```
Player ═══════════════════ Entity
         (Tether Bond)
         
Health drains continuously
↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓ ↓
```

**Key Points:**
- There is no mana—tethering costs your health
- Higher tier entities cost more health
- Affinity reduces tether cost (up to 50% at Ascended level)
- If health reaches zero while tethered, the bond snaps violently

**Tether States:**

| State | Trigger | Result |
|-------|---------|--------|
| Active | `InitiateTether()` | Entity summoned, health drain begins |
| Clean Sever | `SeverTetherCleanly()` | Entity dismissed peacefully, affinity bonus |
| Broken | Health depleted | Entity goes Rampant, betrayal recorded |

### 2. Temperament System

Each entity has a **Temperament**—behavioral requirements that must be satisfied:

| Temperament | Requirement | Example Entity |
|-------------|-------------|----------------|
| **Aggressive** | Must attack frequently | Ignis Mater |
| **Passive** | Must avoid combat | Aqua Pater |
| **Rhythmic** | Must act in patterns | Terra Mater |

**Consequences of Violation:**
- Immediate punishment damage
- Reduced affinity gain
- Potential tether strain

**Meeting Temperament:**
- Bonus affinity gain (+0.2/sec)
- No punishment damage
- Stronger bond formation

### 3. Affinity System

**Affinity** tracks your relationship with each entity type over time:

```
Hostile → Stranger → Acquainted → Bonded → Devoted → Ascended
  ↑                                                      ↑
Betrayal                                           Special Ability
penalty                                               unlocked
```

**Affinity Levels:**

| Level | Threshold | Tether Cost | Special |
|-------|-----------|-------------|---------|
| Hostile | 3+ betrayals | +50% | Entity remembers treachery |
| Stranger | 0-19% | Normal | Default state |
| Acquainted | 20-39% | -10% | Recognition |
| Bonded | 40-69% | -20% | Trust established |
| Devoted | 70-99% | -35% | Deep connection |
| Ascended | 100% | -50% | Special ability unlocked |

**Gaining Affinity:**
- Continuous tethering: +0.5/second
- Meeting temperament: +0.2/second bonus
- Clean sever: +5 bonus
- Heir bonus: 1.5x gain rate

**Losing Affinity:**
- Betrayal (tether break): -15
- Heir betrayal: -5 (they're forgiving)

## Entity Hierarchy Deep Dive

### Parents (Tier 0)

The **Progenitors** are the most powerful magical entities:

```
┌─────────────────────────────────────────────────┐
│                    PARENTS                       │
│                                                  │
│  • Highest tether cost (8-10/sec base)          │
│  • Global environmental effects                  │
│  • Complex temperament requirements              │
│  • Go Rampant when tether breaks                │
│  • Unique special abilities at Ascended         │
└─────────────────────────────────────────────────┘
```

**Current Parents:**

| Entity | Element | Temperament | Special Ability |
|--------|---------|-------------|-----------------|
| Ignis Mater | Fire | Aggressive | Inferno Embrace (AoE damage + invuln) |
| Aqua Pater | Water | Passive | Tidal Sanctuary (healing zone) |
| Terra Mater | Earth | Rhythmic | Earthen Bulwark (damage shield) |

### Scions (Tier 1)

**Scions** are the direct descendants of Parents:

```
┌─────────────────────────────────────────────────┐
│                    SCIONS                        │
│                                                  │
│  • Moderate tether cost (5/sec base)            │
│  • Local environmental effects                   │
│  • Simpler temperament requirements              │
│  • Shorter, less severe Rampant state           │
│  • Grant 50% affinity to parent lineage         │
└─────────────────────────────────────────────────┘
```

**Key Feature:** Tethering with a Scion also builds affinity with their Parent. This encourages players to "train" with Scions before attempting to bond with powerful Parents.

### Heirs (Tier 2)

**Heirs** are gentle, forgiving magical beings:

```
┌─────────────────────────────────────────────────┐
│                    HEIRS                         │
│                                                  │
│  • Lowest tether cost (2/sec base)              │
│  • Minimal environmental effects                 │
│  • Very forgiving temperament                    │
│  • Do NOT go Rampant (simply fade away)         │
│  • Grant 30% affinity to ancestral lineage      │
│  • 1.5x affinity gain rate                      │
│  • Only -5 betrayal penalty (vs -15)            │
└─────────────────────────────────────────────────┘
```

**Key Feature:** Heirs are designed for new players or for building relationships safely. They bond quickly and forgive easily.

## The Rampant State

When a Parent's tether breaks (not cleanly severed), they enter a **Rampant** state:

```
Tether Breaks → Entity Unbound → Rampant State Active
                                        │
                ┌───────────────────────┼───────────────────────┐
                │                       │                       │
                ▼                       ▼                       ▼
           Aggressive              Chaotic               Vengeful
        (Attacks nearest)    (Random behavior)    (Targets player)
```

**Rampant Behaviors:**

| Type | Description | Example Entity |
|------|-------------|----------------|
| Aggressive | Attacks nearest target | Ignis Mater |
| Chaotic | Moves and attacks randomly | Aqua Pater |
| Vengeful | Specifically targets the betrayer | - |
| Destructive | Destroys environment objects | Terra Mater |

**Duration:** 15-45 seconds depending on entity power

**Escape Options:**
1. Flee the area
2. Wait for Rampant state to expire
3. Attempt to rebind (risky—may take damage)

## Cross-Lineage Relationships

The affinity system includes **lineage bonuses**:

```
Tethering Candlelight Heir (Ignis lineage)
          │
          ├── 100% affinity to Candlelight
          │
          └── 30% affinity to Ignis Mater
              (ancestral lineage bonus)
```

This creates a natural progression:
1. Start with Heirs (safe, forgiving)
2. Graduate to Scions (moderate challenge)
3. Attempt Parents (full power, full risk)

Each step builds affinity with the entities above, making future tethers easier.

## Environmental Shifts

When tethered, entities alter the game world:

| Entity | Environmental Effect |
|--------|---------------------|
| Ignis Mater | Red ambient light, heat hazards |
| Aqua Pater | World floods, movement slowed |
| Terra Mater | Gravity increased, structures emerge |
| Ember Scion | Local warm glow |
| Candlelight Heir | Soft point light, comfort effect |

Effects scale with entity tier:
- Parents: Global effects
- Scions: Local area (15m radius)
- Heirs: Immediate vicinity (5m radius)

## Best Practices

### For New Players:
1. Start with Heirs to learn mechanics safely
2. Focus on meeting temperament requirements
3. Always sever cleanly when health gets low
4. Build affinity gradually before attempting Parents

### For Experienced Players:
1. Use Heir/Scion training to build Parent affinity
2. Master temperament patterns for each entity
3. Push tether duration for maximum affinity
4. Unlock special abilities for powerful combinations

### For Multiplayer (Custody Battle):
1. Both players drain faster during battles
2. Temperament mastery determines the winner
3. Strategic withdrawal is valid (avoid backlash damage)

---

[← Back to Home](Home) | [Tether System →](Tether-System)
