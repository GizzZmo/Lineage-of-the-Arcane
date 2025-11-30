# Magic Parent Base Class

The **MagicParent** class is the abstract base class for all magical entities in Lineage of the Arcane.

## Overview

Every magical entity—from the most powerful Parents to the gentlest Heirs—inherits from MagicParent. This class defines:

- Core entity properties (name, cost, defiance)
- Rampant state configuration
- Affinity system integration
- Abstract methods for temperament and environmental effects

## Script Reference

**Location:** `Assets/Scripts/Core/MagicParent.cs`

### Class Definition

```csharp
[RequireComponent(typeof(RampantState))]
public abstract class MagicParent : MonoBehaviour
```

The `RequireComponent` attribute ensures every MagicParent has a RampantState component.

## Properties

### Entity Stats

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `entityName` | `string` | - | Display name of the entity |
| `tetherCostPerSecond` | `float` | 5.0 | Base health drain per second |
| `defianceLevel` | `float` | 0 | Chance to ignore orders (0-100) |

### Rampant Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `rampantBehavior` | `RampantBehavior` | Aggressive | How entity behaves when rampant |
| `rampantDuration` | `float` | 30 | Seconds in rampant state |
| `rampantDamage` | `float` | 15 | Damage per attack while rampant |

### Affinity System

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `entityId` | `string` | ClassName | Unique identifier for affinity tracking |
| `isTemperamentSatisfied` | `bool` | true | Whether temperament is currently met |
| `modifiedTetherCost` | `float` | - | Tether cost after affinity modifier |
| `hasSpecialAbility` | `bool` | false | Whether special ability is unlocked |

### References

| Property | Type | Description |
|----------|------|-------------|
| `boundPlayer` | `PlayerController` | Currently tethered player |
| `rampantState` | `RampantState` | Attached rampant state component |

## Abstract Methods

These methods **must** be implemented by all derived classes:

### ApplyEnvironmentalShift

```csharp
protected abstract void ApplyEnvironmentalShift();
```

Changes the game world when the entity is summoned.

**Example implementations:**
```csharp
// Ignis Mater
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = Color.red;
    Debug.Log("The world heats up. Ignis is watching.");
}

// Aqua Pater
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = new Color(0.2f, 0.4f, 0.8f);
    boundPlayer.moveSpeed *= 0.6f; // Slow movement
}
```

### CheckTemperament

```csharp
public abstract void CheckTemperament();
```

Validates player behavior against entity requirements.

**Example implementations:**
```csharp
// Aggressive (Ignis Mater)
public override void CheckTemperament()
{
    if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
    {
        SetTemperamentSatisfied(false);
        PunishPlayer();
    }
    else
    {
        SetTemperamentSatisfied(true);
    }
}

// Passive (Aqua Pater)
public override void CheckTemperament()
{
    if (Time.time - boundPlayer.lastAttackTime < aggressionThreshold)
    {
        SetTemperamentSatisfied(false);
        PunishPlayer();
    }
    else
    {
        SetTemperamentSatisfied(true);
    }
}
```

## Virtual Methods

These methods can be overridden for custom behavior:

### OnSummon

```csharp
public virtual void OnSummon(PlayerController player)
```

Called when a tether is initiated.

**Default behavior:**
1. Store player reference
2. Apply affinity-based cost modifier
3. Check for special ability unlock
4. Call `ApplyEnvironmentalShift()`
5. Log summon message

### OnTetherMaintained

```csharp
public virtual void OnTetherMaintained(float deltaTime)
```

Called every frame while tethered. Updates affinity.

### OnTetherBroken

```csharp
public virtual void OnTetherBroken()
```

Called when tether breaks unexpectedly (health depleted).

**Default behavior:**
1. Record betrayal in AffinitySystem
2. Enter rampant state
3. Clear bound player reference

### OnTetherSeveredCleanly

```csharp
public virtual void OnTetherSeveredCleanly()
```

Called when tether is manually severed safely.

**Default behavior:**
1. Grant affinity bonus via AffinitySystem
2. Clear bound player reference

### ActivateSpecialAbility

```csharp
public virtual void ActivateSpecialAbility()
```

Activates the entity's special ability (if unlocked).

### OnSpecialAbilityAvailable

```csharp
protected virtual void OnSpecialAbilityAvailable()
```

Called when the special ability becomes available through affinity.

## Utility Methods

### GetModifiedTetherCost

```csharp
public float GetModifiedTetherCost()
```

Returns the tether cost after affinity modifiers are applied.

### GetAffinityInfo

```csharp
public string GetAffinityInfo()
```

Returns a summary string of affinity data for debugging/UI.

### GetAffinityLevel

```csharp
public AffinityLevel GetAffinityLevel()
```

Returns the current affinity level with this entity.

### IsRampant

```csharp
public bool IsRampant()
```

Returns whether the entity is currently in a rampant state.

### GetBoundPlayer

```csharp
public PlayerController GetBoundPlayer()
```

Returns the currently bound player, or null if not tethered.

## Protected Methods

### SetTemperamentSatisfied

```csharp
protected void SetTemperamentSatisfied(bool satisfied)
```

Sets the temperament satisfaction state. Call from CheckTemperament().

### UpdateTetherCostFromAffinity

```csharp
protected void UpdateTetherCostFromAffinity()
```

Recalculates tether cost based on current affinity level.

### ConfigureRampantState

```csharp
protected virtual void ConfigureRampantState()
```

Transfers rampant configuration to the RampantState component.

## Lifecycle

### Awake

```csharp
protected virtual void Awake()
{
    rampantState = GetComponent<RampantState>();
    ConfigureRampantState();
    
    if (string.IsNullOrEmpty(entityId))
    {
        entityId = GetType().Name;
    }
}
```

### Typical Derived Class Start

```csharp
void Start()
{
    entityName = "Entity Name";
    entityId = "EntityId";
    tetherCostPerSecond = 10.0f;
    
    rampantBehavior = RampantBehavior.Aggressive;
    rampantDuration = 45f;
    rampantDamage = 20f;
    
    ConfigureRampantState();
}
```

## Creating a New Entity

### Step 1: Create the Class

```csharp
using UnityEngine;

public class NewParent : MagicParent
{
    [Header("New Parent Settings")]
    public float myCustomSetting = 5f;
    public Color ambientColor = Color.green;
    
    void Start()
    {
        entityName = "New Parent, The Placeholder";
        entityId = "NewParent";
        tetherCostPerSecond = 8.0f;
        
        rampantBehavior = RampantBehavior.Vengeful;
        rampantDuration = 30f;
        rampantDamage = 15f;
        
        ConfigureRampantState();
    }
}
```

### Step 2: Implement Abstract Methods

```csharp
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = ambientColor;
    Debug.Log("New Parent alters the world!");
}

public override void CheckTemperament()
{
    if (boundPlayer == null) return;
    
    // Your temperament logic here
    bool satisfied = CheckYourCondition();
    SetTemperamentSatisfied(satisfied);
    
    if (!satisfied)
    {
        boundPlayer.TakeDamage(5f);
    }
}
```

### Step 3: (Optional) Add Special Ability

```csharp
protected override void OnSpecialAbilityAvailable()
{
    Debug.Log("Special ability unlocked!");
}

public override void ActivateSpecialAbility()
{
    if (!hasSpecialAbility)
    {
        Debug.LogWarning("Ability not unlocked!");
        return;
    }
    
    // Your ability implementation
    DoSpecialThing();
}
```

## Class Hierarchy

```
MagicParent (abstract)
├── IgnisMater (Tier 0 - Aggressive)
├── AquaPater (Tier 0 - Passive)
├── TerraMater (Tier 0 - Rhythmic)
├── Scion (abstract - Tier 1)
│   └── EmberScion
└── Heir (abstract - Tier 2)
    └── CandlelightHeir
```

## Related Documentation

- [[Parents|Parent Entities (Tier 0)]] - Full implementations
- [[Scions|Scion Entities (Tier 1)]] - Mid-tier entities
- [[Heirs|Heir Entities (Tier 2)]] - Gentle entities
- [[Temperament-System|Temperament System]] - Behavioral requirements
- [[Rampant-System|Rampant System]] - Break behavior

---

[← Temperament System](Temperament-System) | [Parent Entities →](Parents)
