# Rampant System

The Rampant System handles the hostile AI behavior that occurs when a Parent's tether breaks unexpectedly.

## Overview

When a **Parent** or **Scion** entity's tether breaks (not cleanly severed), the entity enters a **Rampant state** where it becomes unbound from the player and may act hostilely.

**Note:** Heirs do NOT enter a Rampant state—they simply fade away.

## Script Reference

**Location:** `Assets/Scripts/Core/RampantState.cs`

### Behavior Enum

```csharp
public enum RampantBehavior
{
    Aggressive,   // Attacks the nearest target
    Chaotic,      // Moves and attacks randomly
    Vengeful,     // Specifically targets the betrayer
    Destructive   // Destroys environment objects
}
```

## Configuration

### Inspector Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `behavior` | `RampantBehavior` | Aggressive | How the entity behaves while rampant |
| `rampantDuration` | `float` | 30s | How long the state lasts |
| `attackInterval` | `float` | 2s | Time between attacks |
| `damagePerAttack` | `float` | 15 | Damage dealt per attack |
| `detectionRadius` | `float` | 10m | Range to detect targets |
| `moveSpeed` | `float` | 4 | Movement speed while rampant |
| `targetRefreshInterval` | `float` | 0.5s | Time between target searches |

### Runtime State

| Property | Type | Description |
|----------|------|-------------|
| `isRampant` | `bool` | Whether rampant state is active |
| `rampantStartTime` | `float` | When rampant state began |
| `lastKnownPlayerPosition` | `Transform` | Last position of the betraying player |

## Component Setup

RampantState is automatically added to any MagicParent via `RequireComponent`:

```csharp
[RequireComponent(typeof(RampantState))]
public abstract class MagicParent : MonoBehaviour
```

Parents configure their RampantState in `Start()`:

```csharp
void Start()
{
    rampantBehavior = RampantBehavior.Aggressive;
    rampantDuration = 45f;
    rampantDamage = 20f;
    ConfigureRampantState();
}
```

## Behavior Types

### Aggressive

Attacks the **nearest target** (including the betraying player).

```csharp
void PerformAggressiveBehavior()
{
    // Find nearest target every 0.5 seconds
    if (Time.time - lastTargetRefreshTime > targetRefreshInterval)
    {
        FindNearestTarget();
        lastTargetRefreshTime = Time.time;
    }
    MoveTowardsTarget();
    TryAttackTarget();
}
```

**Used by:** Ignis Mater

### Chaotic

Moves **randomly** and attacks **unpredictably**.

```csharp
void PerformChaoticBehavior()
{
    // 70% chance to move randomly each frame
    if (Random.value > 0.7f)
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)
        ).normalized;
        transform.position += randomDirection * moveSpeed * Time.deltaTime;
    }
    
    // 50% chance to perform area attack when off cooldown
    if (Time.time - lastAttackTime > attackInterval && Random.value > 0.5f)
    {
        PerformAreaAttack();
    }
}
```

**Used by:** Aqua Pater, Scions

### Vengeful

**Specifically targets** the player who broke the tether.

```csharp
void PerformVengefulBehavior()
{
    if (lastKnownPlayerPosition != null)
    {
        currentTarget = lastKnownPlayerPosition;
        MoveTowardsTarget();
        TryAttackTarget();
    }
}
```

**Characteristics:**
- Only pursues the betrayer
- Ignores other targets
- Most dangerous to the responsible player

### Destructive

**Destroys environment objects** rather than chasing players.

```csharp
void PerformDestructiveBehavior()
{
    // Move towards any target if available
    if (currentTarget != null)
    {
        MoveTowardsTarget();
    }
    // Perform area attacks to damage environment
    if (Time.time - lastAttackTime > attackInterval)
    {
        PerformAreaAttack();
    }
}
```

**Used by:** Terra Mater

## Combat System

### Target Detection

```csharp
void FindNearestTarget()
{
    Collider[] colliders = Physics.OverlapSphere(
        transform.position, 
        detectionRadius
    );
    
    float nearestDistance = float.MaxValue;
    Transform nearestTarget = null;
    
    foreach (Collider col in colliders)
    {
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position, 
                col.transform.position
            );
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = col.transform;
            }
        }
    }
    
    currentTarget = nearestTarget ?? lastKnownPlayerPosition;
}
```

### Attack Execution

#### Single Target Attack

```csharp
void PerformAttack(Transform target)
{
    lastAttackTime = Time.time;
    
    PlayerController player = target.GetComponent<PlayerController>();
    if (player != null)
    {
        player.TakeDamage(damagePerAttack);
    }
}
```

#### Area Attack

```csharp
void PerformAreaAttack()
{
    lastAttackTime = Time.time;
    
    Collider[] colliders = Physics.OverlapSphere(
        transform.position, 
        detectionRadius / 2f
    );
    
    foreach (Collider col in colliders)
    {
        PlayerController player = col.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damagePerAttack * 0.5f);
        }
    }
}
```

## Lifecycle

### Entering Rampant State

```csharp
public void EnterRampantState(Transform playerPosition)
{
    isRampant = true;
    rampantStartTime = Time.time;
    lastKnownPlayerPosition = playerPosition;
    
    OnRampantEnter();
}
```

Called from `MagicParent.OnTetherBroken()`:

```csharp
public virtual void OnTetherBroken()
{
    // Record betrayal
    AffinitySystem.Instance.OnTetherBetrayal(entityId);
    
    // Enter rampant state
    if (rampantState != null && boundPlayer != null)
    {
        rampantState.EnterRampantState(boundPlayer.transform);
    }
}
```

### Duration Check

```csharp
void CheckRampantExpiry()
{
    if (Time.time - rampantStartTime > rampantDuration)
    {
        ExitRampantState();
    }
}
```

### Exiting Rampant State

```csharp
public void ExitRampantState()
{
    isRampant = false;
    OnRampantExit();
}
```

## Rebinding

Players can attempt to rebind a Rampant entity:

```csharp
public bool AttemptRebind(PlayerController player)
{
    if (!isRampant) return false;
    
    // Success chance based on player health
    float rebindChance = player.currentHealth / player.maxHealth;
    
    if (Random.value < rebindChance)
    {
        ExitRampantState();
        return true;
    }
    else
    {
        // Failed rebind causes damage
        player.TakeDamage(damagePerAttack * 0.5f);
        return false;
    }
}
```

**Risk/Reward:**
- Higher health = Higher success chance
- Failure = Take damage
- Success = Entity calms down

## Entity-Specific Configuration

### Ignis Mater (Fire)

```csharp
rampantBehavior = RampantBehavior.Aggressive;
rampantDuration = 45f;  // Longer duration
rampantDamage = 20f;    // High damage
```

### Aqua Pater (Water)

```csharp
rampantBehavior = RampantBehavior.Chaotic;
rampantDuration = 35f;
rampantDamage = 15f;
```

### Terra Mater (Earth)

```csharp
rampantBehavior = RampantBehavior.Destructive;
rampantDuration = 40f;
rampantDamage = 18f;
```

### Ember Scion (Tier 1)

```csharp
rampantBehavior = RampantBehavior.Chaotic;
rampantDuration = 15f;  // Much shorter
rampantDamage = 10f;    // Less damage
```

### Heirs (Tier 2)

```csharp
// Heirs do NOT go rampant
public override void OnTetherBroken()
{
    // Simply fade away
    Debug.Log($"Heir {entityName} fades away with a sad expression...");
    
    // Reduced betrayal penalty
    AffinitySystem.Instance.OnTetherBetrayalWithPenalty(entityId, 5f);
    
    // Do NOT call base.OnTetherBroken() - no rampant state
}
```

## Survival Strategies

When facing a Rampant entity:

### 1. Flee

```
Rampant Duration: 15-45 seconds
Move Speed: 4-5 units/second
Detection Radius: 10m

Strategy: Stay outside detection radius until duration expires
```

### 2. Wait It Out

```
Find cover
Heal if possible
Wait for rampantDuration to expire
Entity becomes dormant
```

### 3. Attempt Rebind

```
Risk: Damage on failure
Reward: Immediate resolution
Success Rate: currentHealth / maxHealth

Best when: Health is high
Avoid when: Health is low
```

## Audio Integration

```csharp
void OnRampantEnter()
{
    AudioManager.Instance.PlayRampantStart();
}

void OnRampantExit()
{
    AudioManager.Instance.PlayRampantEnd();
}
```

## Visual Effects

During Rampant state, environmental effects often intensify:

```csharp
// Ignis Mater example
public override void OnTetherBroken()
{
    base.OnTetherBroken();
    
    // Intensify environmental effects
    RenderSettings.ambientLight = Color.red * 2f;
}
```

## Usage Example

### Checking Rampant Status

```csharp
public class RampantWarning : MonoBehaviour
{
    public MagicParent[] nearbyParents;
    
    void Update()
    {
        foreach (var parent in nearbyParents)
        {
            if (parent.IsRampant())
            {
                ShowWarning(parent.entityName);
            }
        }
    }
}
```

### Safe Tether Management

```csharp
public class SafeTethering : MonoBehaviour
{
    public TetherSystem tetherSystem;
    
    void Update()
    {
        if (tetherSystem.isTethered)
        {
            float healthPercent = tetherSystem.player.currentHealth / 
                                  tetherSystem.player.maxHealth;
            
            // Sever before health is too low
            if (healthPercent < 0.25f)
            {
                Debug.Log("Health critical! Severing to avoid Rampant state.");
                tetherSystem.SeverTether();
            }
        }
    }
}
```

## Performance Considerations

The RampantState includes performance optimizations:

```csharp
// Target caching to avoid per-frame searches
public float targetRefreshInterval = 0.5f;

void PerformAggressiveBehavior()
{
    if (Time.time - lastTargetRefreshTime > targetRefreshInterval)
    {
        FindNearestTarget(); // Only every 0.5s
        lastTargetRefreshTime = Time.time;
    }
}
```

## Related Systems

- [[Tether-System|Tether System]] - What triggers Rampant state
- [[Affinity-System|Affinity System]] - Betrayal tracking
- [[Parents|Parent Entities]] - Entity-specific configurations
- [[Audio-System|Audio System]] - Rampant state sounds

---

[← Affinity System](Affinity-System) | [Temperament System →](Temperament-System)
