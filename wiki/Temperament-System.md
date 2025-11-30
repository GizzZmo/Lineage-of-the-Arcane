# Temperament System

The Temperament System defines the behavioral requirements that each magical entity demands from the player during a tether.

## Overview

Every magical entity has a **Temperament**—a set of behavioral requirements the player must satisfy while tethered. Violating these requirements results in punishment, while meeting them grants bonus affinity.

## Temperament Types

| Type | Requirement | Punishment | Example Entity |
|------|-------------|------------|----------------|
| **Aggressive** | Must attack frequently | Damage for hesitation | Ignis Mater |
| **Passive** | Must avoid combat | Damage for attacking | Aqua Pater |
| **Rhythmic** | Must act in patterns | Damage for inconsistency | Terra Mater |

## Implementation

Each entity implements its temperament check in `CheckTemperament()`:

```csharp
public abstract class MagicParent : MonoBehaviour
{
    protected bool isTemperamentSatisfied = true;
    
    // Called every frame while tethered
    public abstract void CheckTemperament();
    
    protected void SetTemperamentSatisfied(bool satisfied)
    {
        isTemperamentSatisfied = satisfied;
    }
}
```

## Aggressive Temperament

**Ignis Mater** demands constant aggression.

### Requirements

- Player must attack frequently
- `hesitationThreshold` defines maximum time without attacking (default: 3 seconds)

### Implementation

```csharp
public class IgnisMater : MagicParent
{
    public float hesitationThreshold = 3.0f;
    public float punishmentDamage = 5.0f;
    
    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Check time since last attack
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
    
    void PunishPlayer()
    {
        boundPlayer.TakeDamage(punishmentDamage);
        Debug.Log("Ignis burns you for your hesitation!");
    }
}
```

### Strategy

```
Keep attacking every 2-3 seconds
Don't wait too long between attacks
Punishment: 5 damage each check
```

## Passive Temperament

**Aqua Pater** demands absolute pacifism.

### Requirements

- Player must not attack
- `aggressionThreshold` defines cooldown after attacking (default: 2 seconds)

### Implementation

```csharp
public class AquaPater : MagicParent
{
    public float aggressionThreshold = 2.0f;
    public float punishmentDamage = 4.0f;
    private float lastAggressionTime;
    
    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Check if player attacked recently
        if (Time.time - boundPlayer.lastAttackTime < aggressionThreshold)
        {
            SetTemperamentSatisfied(false);
            if (Time.time - lastAggressionTime > 1.0f)
            {
                PunishPlayer();
                lastAggressionTime = Time.time;
            }
        }
        else
        {
            SetTemperamentSatisfied(true);
        }
    }
    
    void PunishPlayer()
    {
        boundPlayer.TakeDamage(punishmentDamage);
        Debug.Log("Aqua Pater drowns you for your violence!");
    }
}
```

### Strategy

```
Do not attack while tethered
Wait 2+ seconds after any attack before tethering
Punishment: 4 damage every second during aggression
```

## Rhythmic Temperament

**Terra Mater** demands consistent, patterned actions.

### Requirements

- Actions must be performed at consistent intervals
- `rhythmWindow` defines expected interval (default: 3 seconds)
- `rhythmTolerance` defines acceptable deviation (default: 0.5 seconds)

### Implementation

```csharp
public class TerraMater : MagicParent
{
    public float rhythmWindow = 3.0f;
    public float rhythmTolerance = 0.5f;
    public float punishmentDamage = 6.0f;
    
    private float lastActionTime;
    private int rhythmStreak = 0;
    private bool hasStartedRhythm = false;
    
    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        if (boundPlayer.lastAttackTime > lastActionTime)
        {
            float interval = boundPlayer.lastAttackTime - lastActionTime;
            
            if (hasStartedRhythm)
            {
                float deviation = Mathf.Abs(interval - rhythmWindow);
                
                if (deviation <= rhythmTolerance)
                {
                    // Good rhythm!
                    rhythmStreak++;
                    SetTemperamentSatisfied(true);
                    Debug.Log($"Rhythm streak: {rhythmStreak}");
                }
                else
                {
                    // Rhythm broken!
                    SetTemperamentSatisfied(false);
                    PunishPlayer();
                    rhythmStreak = 0;
                }
            }
            else
            {
                hasStartedRhythm = true;
                Debug.Log("Terra Mater begins measuring...");
            }
            
            lastActionTime = boundPlayer.lastAttackTime;
        }
        
        // Punish for silence
        float timeoutThreshold = rhythmWindow + rhythmTolerance * 2f;
        if (hasStartedRhythm && Time.time - lastActionTime > timeoutThreshold)
        {
            SetTemperamentSatisfied(false);
            PunishPlayer();
            hasStartedRhythm = false;
            rhythmStreak = 0;
        }
    }
}
```

### Strategy

```
Attack every 3 seconds (±0.5s tolerance)
Maintain consistent timing
Don't wait too long between attacks
Build rhythm streak for bonus effects
Punishment: 6 damage per violation
```

### Rhythm Bonus

```csharp
public void ApplyRhythmBonus()
{
    if (rhythmStreak >= 5 && boundPlayer != null)
    {
        float reduction = tetherCostPerSecond * 0.1f * (rhythmStreak / 5);
        Debug.Log($"Rhythm bonus: {reduction} reduced cost");
    }
}
```

## Tier-Based Temperament

Lower-tier entities have more forgiving temperament checks.

### Scions (Tier 1)

```csharp
public abstract class Scion : MagicParent
{
    public float temperamentTolerance = 5f;   // Longer grace period
    public float punishmentMultiplier = 0.5f; // Half damage
    
    protected void ApplyPunishment(float baseDamage)
    {
        float actualDamage = baseDamage * punishmentMultiplier;
        boundPlayer.TakeDamage(actualDamage);
    }
}
```

### Heirs (Tier 2)

```csharp
public abstract class Heir : MagicParent
{
    public float temperamentGracePeriod = 10f;  // Very long grace
    public float punishmentMultiplier = 0.2f;   // Minimal damage
    public bool isForgiving = true;             // Often no punishment
    
    public override void CheckTemperament()
    {
        // Heirs are always satisfied by default
        SetTemperamentSatisfied(true);
        
        if (isForgiving)
        {
            PerformGentleTemperamentCheck();
        }
    }
    
    protected void ApplyMildPunishment(float baseDamage)
    {
        if (!isForgiving)
        {
            float actualDamage = baseDamage * punishmentMultiplier;
            boundPlayer.TakeDamage(actualDamage);
        }
        else
        {
            Debug.Log("Heir looks sad but forgives you.");
        }
    }
}
```

## Affinity Integration

Temperament satisfaction affects affinity gain:

```csharp
// In TetherSystem.MaintainTether()
activeSummon.OnTetherMaintained(Time.deltaTime);

// In MagicParent
public virtual void OnTetherMaintained(float deltaTime)
{
    // isTemperamentSatisfied affects bonus
    AffinitySystem.Instance.AddTetherAffinity(
        entityId, 
        deltaTime, 
        isTemperamentSatisfied
    );
}

// In AffinitySystem
public void AddTetherAffinity(string entityId, float deltaTime, bool temperamentSatisfied)
{
    float gain = affinityGainRate * deltaTime;  // 0.5/sec base
    
    if (temperamentSatisfied)
    {
        gain += temperamentBonus * deltaTime;   // +0.2/sec bonus
    }
    
    // Apply gain...
}
```

### Gain Rates

| Condition | Affinity/Second |
|-----------|-----------------|
| Temperament Violated | 0.5 |
| Temperament Satisfied | 0.7 (+0.2 bonus) |

## Visual/Audio Feedback

### UI Integration

```csharp
public class TetherDisplayUI : MonoBehaviour
{
    public enum TemperamentStatus
    {
        Pleased,
        Neutral,
        Displeased,
        Angry
    }
    
    public void SetTemperamentStatus(TemperamentStatus status)
    {
        // Update colors and text
        temperamentIndicator.color = GetTemperamentColor(status);
        temperamentStatusText.text = GetTemperamentText(status);
    }
}
```

### Audio Warnings

```csharp
// When temperament is violated
AudioManager.Instance.PlayViolationWarning();

// When punishment is applied
AudioManager.Instance.PlayPunishment();
```

## Usage Examples

### Monitoring Temperament

```csharp
public class TemperamentMonitor : MonoBehaviour
{
    public TetherSystem tetherSystem;
    public TetherDisplayUI tetherUI;
    
    void Update()
    {
        if (tetherSystem.isTethered && tetherSystem.activeSummon != null)
        {
            MagicParent summon = tetherSystem.activeSummon;
            
            // Determine UI status based on recent behavior
            if (summon is IgnisMater ignis)
            {
                float timeSinceAttack = Time.time - tetherSystem.player.lastAttackTime;
                
                if (timeSinceAttack < 1f)
                    tetherUI.SetTemperamentStatus(TetherDisplayUI.TemperamentStatus.Pleased);
                else if (timeSinceAttack < 2f)
                    tetherUI.SetTemperamentStatus(TetherDisplayUI.TemperamentStatus.Neutral);
                else if (timeSinceAttack < 3f)
                    tetherUI.SetTemperamentStatus(TetherDisplayUI.TemperamentStatus.Displeased);
                else
                    tetherUI.SetTemperamentStatus(TetherDisplayUI.TemperamentStatus.Angry);
            }
        }
    }
}
```

### Temperament-Based Strategy

```csharp
public class AutoTetherHelper : MonoBehaviour
{
    public TetherSystem tetherSystem;
    
    void Update()
    {
        if (!tetherSystem.isTethered) return;
        
        MagicParent summon = tetherSystem.activeSummon;
        
        // Give hints based on temperament
        if (summon is IgnisMater)
        {
            float timeSinceAttack = Time.time - tetherSystem.player.lastAttackTime;
            if (timeSinceAttack > 2.5f)
            {
                ShowHint("Attack soon or face Ignis's wrath!");
            }
        }
        else if (summon is AquaPater)
        {
            // Warn against attacking
            if (Input.GetMouseButtonDown(0))
            {
                ShowHint("Attacking will anger Aqua Pater!");
            }
        }
    }
}
```

## Summary Table

| Entity | Type | Check Frequency | Grace Period | Punishment |
|--------|------|-----------------|--------------|------------|
| Ignis Mater | Aggressive | Every frame | 3s | 5 damage |
| Aqua Pater | Passive | Every frame | 2s cooldown | 4 damage/sec |
| Terra Mater | Rhythmic | On action | ±0.5s tolerance | 6 damage |
| Ember Scion | Aggressive (mild) | Every 5s | 5s | 1.5 damage |
| Candlelight Heir | None | Every 10s | N/A | None |

## Related Systems

- [[Parents|Parent Entities]] - Specific temperament implementations
- [[Affinity-System|Affinity System]] - Temperament bonus integration
- [[UI-Overview|UI Overview]] - Temperament status display
- [[Audio-System|Audio System]] - Warning sounds

---

[← Rampant System](Rampant-System) | [Magic Parent →](Magic-Parent)
