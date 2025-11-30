# Getting Started Guide

This guide will help you set up and start developing with Lineage of the Arcane.

## Prerequisites

Before you begin, ensure you have the following installed:

- **Unity 2021.3 LTS** or later
- **Git** for version control
- A compatible IDE (Visual Studio, Visual Studio Code, or JetBrains Rider)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/GizzZmo/Lineage-of-the-Arcane.git
cd Lineage-of-the-Arcane
```

### 2. Open in Unity

1. Open Unity Hub
2. Click "Add" and navigate to the cloned repository
3. Select the project folder
4. Open the project with Unity 2021.3 LTS or later

### 3. Initial Setup

Once the project opens:

1. Wait for Unity to import all assets
2. Open the main scene (if available) or create a test scene
3. Add required GameObjects (see below)

## Setting Up a Test Scene

To test the magic system, you'll need to set up a basic scene with the following components:

### Required GameObjects

#### 1. Player Setup

```
Player (GameObject)
├── PlayerController (Script)
├── TetherSystem (Script)
├── Rigidbody (Component)
└── Collider (Component)
```

**PlayerController Configuration:**
- `maxHealth`: 100 (default)
- `maxSanity`: 100 (default)
- `moveSpeed`: 5.0 (default)

#### 2. Affinity System (Singleton)

Create an empty GameObject named "AffinitySystem" and attach:
- `AffinitySystem` script

The system will automatically persist across scenes.

#### 3. Audio Manager (Singleton)

Create an empty GameObject named "AudioManager" and attach:
- `AudioManager` script

Configure audio clips as needed.

#### 4. Magic Parent Entity

Create a GameObject for your magical entity:

```
IgnisMater (GameObject)
├── IgnisMater (Script)
├── RampantState (Script) - Added automatically via RequireComponent
└── Collider (Component)
```

### Connecting the Systems

1. **Link Player to TetherSystem:**
   - Select the TetherSystem component
   - Drag the PlayerController to the `player` field

2. **Initiate a Tether (via code):**

```csharp
// Get references
TetherSystem tether = player.GetComponent<TetherSystem>();
MagicParent parent = FindObjectOfType<IgnisMater>();

// Start the tether
tether.InitiateTether(parent);
```

## Understanding the Core Loop

The basic gameplay loop works as follows:

```
1. Player initiates tether with a Parent
   └── OnSummon() called on Parent
       ├── Affinity modifier applied to tether cost
       ├── Environmental effects activated
       └── Special ability check

2. While tethered (every frame):
   ├── MaintainTether() drains player health
   ├── CheckTemperament() validates player behavior
   ├── OnTetherMaintained() adds affinity
   └── Visual effects update

3. Tether ends via:
   ├── SeverTetherCleanly() → Grants affinity bonus
   ├── BreakTether() (health depleted) → Betrayal penalty
   └── Parent enters Rampant state if broken
```

## Quick Code Examples

### Initiating a Tether

```csharp
public class TetherExample : MonoBehaviour
{
    public TetherSystem tetherSystem;
    public MagicParent targetParent;
    
    void Update()
    {
        // Press E to tether
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!tetherSystem.isTethered)
            {
                tetherSystem.InitiateTether(targetParent);
            }
            else
            {
                tetherSystem.SeverTether();
            }
        }
    }
}
```

### Checking Affinity Level

```csharp
public void CheckRelationship(string entityId)
{
    AffinityLevel level = AffinitySystem.Instance.GetAffinityLevel(entityId);
    float percentage = AffinitySystem.Instance.GetAffinityPercentage(entityId);
    bool hasAbility = AffinitySystem.Instance.IsAbilityUnlocked(entityId);
    
    Debug.Log($"Level: {level}, Progress: {percentage}%, Ability: {hasAbility}");
}
```

### Activating Special Abilities

```csharp
public void UseSpecialAbility(MagicParent parent)
{
    if (parent.GetAffinityLevel() == AffinityLevel.Ascended)
    {
        parent.ActivateSpecialAbility();
    }
}
```

## Testing the Systems

### Testing Tether Drain

1. Set player `maxHealth` to 50 for faster testing
2. Set parent `tetherCostPerSecond` to 10
3. Initiate tether and observe health drain
4. Watch for tether break when health is depleted

### Testing Temperament

For **Ignis Mater** (aggressive temperament):
1. Initiate tether
2. Wait without attacking for `hesitationThreshold` seconds
3. Observe punishment damage

For **Aqua Pater** (passive temperament):
1. Initiate tether
2. Attack during the tether
3. Observe punishment damage

### Testing Affinity

1. Complete multiple successful tethers
2. Check affinity level progression
3. Test with Heirs for faster affinity gain
4. Observe cost reduction at higher levels

## Common Issues

### "Cannot initiate tether: Player is null!"
- Ensure the `player` field on TetherSystem is assigned

### RampantState not triggering
- RampantState is added via `RequireComponent` - check that MagicParent script is present

### Affinity not saving between sessions
- AffinitySystem uses `DontDestroyOnLoad` but doesn't persist to disk by default
- Implement PlayerPrefs or file-based saving for persistence

## Next Steps

- [[Core-Concepts|Learn Core Concepts]] - Understand the game mechanics
- [[Tether-System|Explore Tether System]] - Deep dive into the binding mechanic
- [[API-Reference|API Reference]] - Complete method documentation

---

[← Back to Home](Home)
