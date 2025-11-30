# Tether System

The Tether System is the core mechanic of Lineage of the Arcane, managing the magical bond between players and summoned entities.

## Overview

The **TetherSystem** component handles:
- Initiating and maintaining tethers
- Health drain calculations
- Safe vs. forced tether termination
- Integration with the Affinity System

## Script Reference

**Location:** `Assets/Scripts/Core/TetherSystem.cs`

### Class Definition

```csharp
public class TetherSystem : MonoBehaviour
```

### Public Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `activeSummon` | `MagicParent` | null | Currently tethered entity |
| `player` | `PlayerController` | null | Reference to the player |
| `isTethered` | `bool` | false | Whether a tether is active |
| `tetherColor` | `Color` | Blue | Visual color for tether |
| `tetherWidth` | `float` | 0.1f | Width of tether line |
| `safeSeverThreshold` | `float` | 0.2f | Min health % for safe sever |

### Public Methods

#### InitiateTether

```csharp
public void InitiateTether(MagicParent summon)
```

Initiates a tether between the player and a magical entity.

**Parameters:**
- `summon` - The MagicParent to tether

**Behavior:**
1. Validates player and summon references
2. Sets `isTethered = true`
3. Calls `summon.OnSummon(player)`
4. Retrieves affinity-modified tether cost

**Example:**
```csharp
TetherSystem tether = player.GetComponent<TetherSystem>();
MagicParent ignis = FindObjectOfType<IgnisMater>();
tether.InitiateTether(ignis);
```

#### SeverTether

```csharp
public void SeverTether()
```

Manually severs the tether connection. Determines whether it's a clean sever or forced break based on player health.

**Behavior:**
- If `healthPercent >= safeSeverThreshold`: Clean sever (grants affinity)
- If `healthPercent < safeSeverThreshold`: Counts as a break (betrayal)

#### SeverTetherCleanly

```csharp
public void SeverTetherCleanly()
```

Forces a clean sever regardless of health. Grants affinity bonus.

**Use Case:** When you want to ensure a clean sever even at low health (e.g., special game events).

#### GetTetherSessionInfo

```csharp
public string GetTetherSessionInfo()
```

Returns information about the current tether session.

**Returns:** String containing entity name, duration, cost/sec, and affinity level.

#### GetCurrentTetherCost

```csharp
public float GetCurrentTetherCost()
```

Gets the current tether cost per second after affinity modifiers.

#### GetSessionDuration

```csharp
public float GetSessionDuration()
```

Gets how long the current tether has been active (in seconds).

## Tether Flow

### Lifecycle Diagram

```
          ┌─────────────────┐
          │  No Tether      │
          │  isTethered=F   │
          └────────┬────────┘
                   │
                   │ InitiateTether(summon)
                   ▼
          ┌─────────────────┐
          │  Tether Active  │
          │  isTethered=T   │
          └────────┬────────┘
                   │
         ┌─────────┴─────────┐
         │   Update Loop     │
         │  ┌─────────────┐  │
         │  │MaintainTether│  │
         │  │CheckTemper. │  │
         │  └─────────────┘  │
         └─────────┬─────────┘
                   │
         ┌─────────┴─────────┐
         │                   │
    HealthOK           Health<=cost
         │                   │
         ▼                   ▼
  ┌─────────────┐    ┌─────────────┐
  │SeverTether()│    │BreakTether()│
  └──────┬──────┘    └──────┬──────┘
         │                   │
         ▼                   ▼
  ┌─────────────┐    ┌─────────────┐
  │Clean Sever  │    │  Betrayal   │
  │+5 Affinity  │    │-15 Affinity │
  └──────┬──────┘    │Rampant State│
         │           └──────┬──────┘
         │                   │
         └─────────┬─────────┘
                   │
                   ▼
          ┌─────────────────┐
          │  No Tether      │
          │  isTethered=F   │
          └─────────────────┘
```

### Update Loop (Every Frame)

```csharp
void Update()
{
    if (isTethered && activeSummon != null)
    {
        MaintainTether();           // Drain health
        activeSummon.CheckTemperament(); // Check behavior
        sessionDuration += Time.deltaTime;
    }
}
```

### Health Drain Calculation

```csharp
void MaintainTether()
{
    // Get affinity-modified cost
    currentTetherCost = activeSummon.GetModifiedTetherCost();
    float cost = currentTetherCost * Time.deltaTime;
    
    if (player.currentHealth > cost)
    {
        player.currentHealth -= cost;
        activeSummon.OnTetherMaintained(Time.deltaTime);
    }
    else
    {
        BreakTether(); // Health depleted
    }
}
```

## Cost Calculation

The tether cost is modified by affinity level:

```
Final Cost = Base Cost × Affinity Multiplier
```

### Multiplier Table

| Affinity Level | Multiplier | Effect |
|----------------|------------|--------|
| Hostile | 1.5x | +50% cost |
| Stranger | 1.0x | No change |
| Acquainted | 0.9x | -10% cost |
| Bonded | 0.8x | -20% cost |
| Devoted | 0.65x | -35% cost |
| Ascended | 0.5x | -50% cost |

### Example Calculation

```
Base Cost: 10 HP/sec (Ignis Mater)
Affinity: Devoted (0.65x)
Final Cost: 10 × 0.65 = 6.5 HP/sec
```

## Integration with Other Systems

### Affinity System

```csharp
// On tether initiation
currentTetherCost = activeSummon.GetModifiedTetherCost();

// During maintenance
activeSummon.OnTetherMaintained(Time.deltaTime);
// → Calls AffinitySystem.Instance.AddTetherAffinity()

// On clean sever
activeSummon.OnTetherSeveredCleanly();
// → Calls AffinitySystem.Instance.OnSuccessfulTether()

// On break
activeSummon.OnTetherBroken();
// → Calls AffinitySystem.Instance.OnTetherBetrayal()
```

### Visual Effects

The `TetherVisualEffect` component listens to TetherSystem state:

```csharp
void Update()
{
    if (tetherSystem != null && tetherSystem.isTethered)
    {
        // Activate and update visuals
        UpdateTetherPositions();
        UpdateTetherColor();
        UpdatePulseEffect();
    }
}
```

### Audio Manager

```csharp
// Recommended integration points:
AudioManager.Instance.PlayTetherForm();  // On InitiateTether
AudioManager.Instance.StartTetherPulse(); // While active
AudioManager.Instance.StopTetherPulse();  // On sever
AudioManager.Instance.PlayTetherBreak();  // On break
```

## Safe Sever Threshold

The `safeSeverThreshold` determines when a sever is considered "clean":

```
healthPercent = currentHealth / maxHealth

if (healthPercent >= safeSeverThreshold)
    → Clean Sever (affinity bonus)
else
    → Forced Break (betrayal penalty)
```

**Default:** 0.2 (20% health)

### Adjusting Threshold

Lower threshold = More risk tolerance
Higher threshold = Safer but requires better resource management

```csharp
// For hardcore mode:
tetherSystem.safeSeverThreshold = 0.1f; // 10%

// For casual mode:
tetherSystem.safeSeverThreshold = 0.3f; // 30%
```

## Usage Examples

### Basic Tethering

```csharp
public class PlayerTetherController : MonoBehaviour
{
    public TetherSystem tetherSystem;
    public MagicParent targetParent;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleTether();
        }
    }
    
    void ToggleTether()
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
```

### Emergency Sever on Low Health

```csharp
public class AutoSever : MonoBehaviour
{
    public TetherSystem tetherSystem;
    public float emergencyThreshold = 0.15f;
    
    void Update()
    {
        if (tetherSystem.isTethered)
        {
            float healthPercent = tetherSystem.player.currentHealth / 
                                  tetherSystem.player.maxHealth;
            
            if (healthPercent <= emergencyThreshold)
            {
                Debug.Log("Emergency sever! Health critical.");
                tetherSystem.SeverTether();
            }
        }
    }
}
```

### Monitoring Tether State

```csharp
public class TetherMonitor : MonoBehaviour
{
    public TetherSystem tetherSystem;
    
    void Update()
    {
        if (tetherSystem.isTethered)
        {
            string info = tetherSystem.GetTetherSessionInfo();
            float cost = tetherSystem.GetCurrentTetherCost();
            float duration = tetherSystem.GetSessionDuration();
            
            Debug.Log($"Tether Info: {info}");
            Debug.Log($"Cost/sec: {cost}, Duration: {duration}s");
        }
    }
}
```

## Best Practices

1. **Always check references** before initiating tethers
2. **Monitor player health** and sever before depletion
3. **Use clean severs** when possible for affinity bonuses
4. **Consider entity tier** when planning tether duration
5. **Build affinity** with Heirs first to reduce costs

## Related Systems

- [[Affinity-System|Affinity System]] - Cost modifiers and relationship tracking
- [[Rampant-System|Rampant System]] - What happens when tethers break
- [[Temperament-System|Temperament System]] - Behavioral requirements during tethers
- [[Visual-Effects|Visual Effects]] - Tether visualization

---

[← Core Concepts](Core-Concepts) | [Affinity System →](Affinity-System)
