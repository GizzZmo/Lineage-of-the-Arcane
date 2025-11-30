# Affinity System

The Affinity System tracks and manages player-entity relationships, providing gameplay benefits and consequences based on how players interact with magical beings over time.

## Overview

The **AffinitySystem** is a singleton component that:
- Tracks affinity levels for all entity types
- Calculates tether cost modifiers
- Manages affinity gain and loss
- Fires events for UI and game responses
- Unlocks special abilities at max affinity

## Script Reference

**Location:** `Assets/Scripts/Core/AffinitySystem.cs`

### Related Class: AffinityData

```csharp
[System.Serializable]
public class AffinityData
{
    public string entityId;           // Unique identifier
    public float currentAffinity;     // 0-100 points
    public AffinityLevel level;       // Current level enum
    public int successfulTethers;     // Clean sever count
    public int betrayals;             // Break count
    public float totalTimeTethered;   // Total time in seconds
    public bool hasUnlockedAbility;   // Special ability status
}
```

### Affinity Level Enum

```csharp
public enum AffinityLevel
{
    Hostile = -1,      // 3+ betrayals with low affinity
    Stranger = 0,      // Default state (0-19%)
    Acquainted = 1,    // Recognition (20-39%)
    Bonded = 2,        // Trust established (40-69%)
    Devoted = 3,       // Deep connection (70-99%)
    Ascended = 4       // Maximum + special ability (100%)
}
```

## Singleton Access

The AffinitySystem uses the singleton pattern for global access:

```csharp
// Access from anywhere
AffinitySystem.Instance.GetAffinityLevel("IgnisMater");

// The singleton auto-creates if not present:
if (instance == null)
{
    instance = FindObjectOfType<AffinitySystem>();
    if (instance == null)
    {
        GameObject go = new GameObject("AffinitySystem");
        instance = go.AddComponent<AffinitySystem>();
    }
}
```

## Configuration

### Inspector Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `affinityGainRate` | float | 0.5 | Base affinity per second tethered |
| `betrayalPenalty` | float | 15 | Affinity lost on tether break |
| `temperamentBonus` | float | 0.2 | Bonus per second when meeting requirements |
| `cleanSeverBonus` | float | 5 | Bonus for clean tether termination |

### Cost Multipliers

| Property | Default | Description |
|----------|---------|-------------|
| `hostileCostMultiplier` | 1.5 | +50% tether cost |
| `acquaintedCostMultiplier` | 0.9 | -10% tether cost |
| `bondedCostMultiplier` | 0.8 | -20% tether cost |
| `devotedCostMultiplier` | 0.65 | -35% tether cost |
| `ascendedCostMultiplier` | 0.5 | -50% tether cost |

## Public Methods

### Getting Affinity Data

#### GetAffinityData

```csharp
public AffinityData GetAffinityData(string entityId)
```

Returns the complete affinity record for an entity. Creates a new record if one doesn't exist.

#### GetAffinityLevel

```csharp
public AffinityLevel GetAffinityLevel(string entityId)
```

Returns the current affinity level enum.

#### GetAffinityPercentage

```csharp
public float GetAffinityPercentage(string entityId)
```

Returns the current affinity as a percentage (0-100).

#### GetProgressToNextLevel

```csharp
public float GetProgressToNextLevel(string entityId)
```

Returns progress to the next level (0-1).

**Example:**
```csharp
float progress = AffinitySystem.Instance.GetProgressToNextLevel("IgnisMater");
// Returns 0.5 if halfway through current level
```

### Modifying Affinity

#### AddTetherAffinity

```csharp
public void AddTetherAffinity(string entityId, float deltaTime, bool temperamentSatisfied)
```

Called during tether maintenance to add continuous affinity.

**Parameters:**
- `entityId` - The entity identifier
- `deltaTime` - Time since last update
- `temperamentSatisfied` - Whether temperament requirements are met

**Calculation:**
```
gain = affinityGainRate × deltaTime
if (temperamentSatisfied) gain += temperamentBonus × deltaTime
```

#### OnSuccessfulTether

```csharp
public void OnSuccessfulTether(string entityId)
```

Called when a tether is cleanly severed. Adds the clean sever bonus.

#### OnSuccessfulTetherWithBonus

```csharp
public void OnSuccessfulTetherWithBonus(string entityId, float bonusAmount)
```

Called by entities that grant extra affinity (like Heirs).

#### OnTetherBetrayal

```csharp
public void OnTetherBetrayal(string entityId)
```

Called when a tether breaks unexpectedly. Applies the standard betrayal penalty.

#### OnTetherBetrayalWithPenalty

```csharp
public void OnTetherBetrayalWithPenalty(string entityId, float penalty)
```

Called by entities with custom betrayal penalties (like Heirs with reduced penalty).

### Cost Calculation

#### GetTetherCostMultiplier

```csharp
public float GetTetherCostMultiplier(string entityId)
```

Returns the cost multiplier based on affinity level.

**Example:**
```csharp
float multiplier = AffinitySystem.Instance.GetTetherCostMultiplier("IgnisMater");
float actualCost = baseCost * multiplier;
```

### Utility Methods

#### IsAbilityUnlocked

```csharp
public bool IsAbilityUnlocked(string entityId)
```

Returns whether the special ability is unlocked (Ascended level reached).

#### ResetAffinity

```csharp
public void ResetAffinity(string entityId)
```

Resets affinity data for an entity. Use with caution.

#### GetAffinitySummary

```csharp
public string GetAffinitySummary(string entityId)
```

Returns a formatted debug string with affinity information.

## Events

The AffinitySystem fires events for UI updates and game reactions:

```csharp
// When affinity level changes (up or down)
public System.Action<string, AffinityLevel> OnAffinityLevelChanged;

// When any affinity is gained
public System.Action<string, float> OnAffinityGained;

// When any affinity is lost
public System.Action<string, float> OnAffinityLost;

// When a special ability is unlocked
public System.Action<string> OnAbilityUnlocked;
```

### Subscribing to Events

```csharp
void Start()
{
    AffinitySystem.Instance.OnAffinityLevelChanged += HandleLevelChanged;
    AffinitySystem.Instance.OnAbilityUnlocked += HandleAbilityUnlocked;
}

void OnDestroy()
{
    // Clean up subscriptions
    AffinitySystem instance = FindObjectOfType<AffinitySystem>();
    if (instance != null)
    {
        instance.OnAffinityLevelChanged -= HandleLevelChanged;
        instance.OnAbilityUnlocked -= HandleAbilityUnlocked;
    }
}

void HandleLevelChanged(string entityId, AffinityLevel newLevel)
{
    Debug.Log($"{entityId} is now {newLevel}!");
}

void HandleAbilityUnlocked(string entityId)
{
    Debug.Log($"Special ability unlocked for {entityId}!");
}
```

## Affinity Level Thresholds

```csharp
public const float ACQUAINTED_THRESHOLD = 20f;
public const float BONDED_THRESHOLD = 40f;
public const float DEVOTED_THRESHOLD = 70f;
public const float ASCENDED_THRESHOLD = 100f;
```

### Level Progression

```
0%   20%      40%         70%            100%
|-----|--------|-----------|--------------|
Stranger Acquainted  Bonded    Devoted   Ascended
```

### Hostile State

Hostile is a special state triggered by:
- 3 or more betrayals
- AND affinity below 20%

```csharp
if (betrayals >= 3 && currentAffinity < ACQUAINTED_THRESHOLD)
{
    level = AffinityLevel.Hostile;
}
```

## Entity-Specific Behavior

### Scions (Tier 1)

Scions grant bonus affinity to their parent lineage:

```csharp
// In Scion.OnTetherMaintained():
base.OnTetherMaintained(deltaTime);
string parentId = GetParentEntityId();
AffinitySystem.Instance.AddTetherAffinity(
    parentId, 
    deltaTime * parentAffinityBonus,  // 50% of normal
    isTemperamentSatisfied
);
```

### Heirs (Tier 2)

Heirs have special affinity bonuses:

```csharp
// Faster affinity gain
float bonusTime = deltaTime * affinityGainMultiplier; // 1.5x
AffinitySystem.Instance.AddTetherAffinity(entityId, bonusTime, true);

// Reduced betrayal penalty
AffinitySystem.Instance.OnTetherBetrayalWithPenalty(entityId, 5f); // vs 15

// Bonus on clean sever
AffinitySystem.Instance.OnSuccessfulTetherWithBonus(entityId, 3f);
```

## Usage Examples

### Checking Relationship Status

```csharp
public void DisplayRelationshipStatus(string entityId)
{
    AffinityLevel level = AffinitySystem.Instance.GetAffinityLevel(entityId);
    float percentage = AffinitySystem.Instance.GetAffinityPercentage(entityId);
    float progress = AffinitySystem.Instance.GetProgressToNextLevel(entityId);
    bool hasAbility = AffinitySystem.Instance.IsAbilityUnlocked(entityId);
    
    Debug.Log($"Entity: {entityId}");
    Debug.Log($"Level: {level} ({percentage}%)");
    Debug.Log($"Progress to next: {progress * 100}%");
    Debug.Log($"Ability unlocked: {hasAbility}");
}
```

### Cost Calculation Example

```csharp
public float CalculateActualTetherCost(MagicParent parent)
{
    float baseCost = parent.tetherCostPerSecond;
    float multiplier = AffinitySystem.Instance.GetTetherCostMultiplier(parent.entityId);
    
    return baseCost * multiplier;
}
```

### Building Affinity Through Training

```csharp
// Strategy: Train with Heir → Scion → Parent
public class AffinityTrainer : MonoBehaviour
{
    public void TrainWithLineage(string lineage)
    {
        // Get current parent affinity
        float parentAffinity = AffinitySystem.Instance.GetAffinityPercentage(
            GetParentId(lineage)
        );
        
        // Recommend appropriate tier
        if (parentAffinity < 20)
        {
            Debug.Log("Start with an Heir for safety");
        }
        else if (parentAffinity < 50)
        {
            Debug.Log("Try a Scion for faster progress");
        }
        else
        {
            Debug.Log("You're ready for the Parent!");
        }
    }
}
```

## Persistence

By default, affinity data only persists for the current play session. To save between sessions:

```csharp
// Example save implementation
public void SaveAffinity()
{
    foreach (var kvp in affinityRecords)
    {
        string json = JsonUtility.ToJson(kvp.Value);
        PlayerPrefs.SetString($"Affinity_{kvp.Key}", json);
    }
    PlayerPrefs.Save();
}

public void LoadAffinity()
{
    // Load each known entity type
    string[] entityIds = { "IgnisMater", "AquaPater", "TerraMater" };
    
    foreach (string id in entityIds)
    {
        string json = PlayerPrefs.GetString($"Affinity_{id}", "");
        if (!string.IsNullOrEmpty(json))
        {
            AffinityData data = JsonUtility.FromJson<AffinityData>(json);
            affinityRecords[id] = data;
        }
    }
}
```

## Design Philosophy

The Affinity System is designed to:

1. **Reward Commitment** - Long tethers build stronger bonds
2. **Punish Recklessness** - Breaking tethers damages relationships
3. **Enable Progression** - Start with Heirs, graduate to Parents
4. **Provide Meaningful Choices** - Risk vs. reward in tether duration
5. **Create Memory** - Entities "remember" how you've treated them

---

[← Tether System](Tether-System) | [Rampant System →](Rampant-System)
