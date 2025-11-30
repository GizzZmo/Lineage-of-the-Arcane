# Scion Entities (Tier 1)

Scions are mid-tier magical entities—direct descendants of the Parents with moderate power and simpler requirements.

## Overview

Scions serve as a training ground for players learning to work with a particular lineage. They have:

- **Moderate tether cost** (5 HP/sec base)
- **Local environmental effects** (15m radius)
- **Simpler temperament requirements**
- **Shorter, less severe rampant state**
- **Lineage affinity bonus** (50% to parent)

## Scion Base Class

**Location:** `Assets/Scripts/Entities/Tiers/Scion.cs`

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `parentLineage` | `string` | - | Which Parent they descend from |
| `localEffectRadius` | `float` | 15 | Range of environmental effects |
| `scionAffinityBonus` | `float` | 0.8 | Tether cost multiplier (20% reduction) |
| `temperamentTolerance` | `float` | 5 | Seconds before temperament check |
| `punishmentMultiplier` | `float` | 0.5 | Reduced punishment (50%) |
| `parentAffinityBonus` | `float` | 0.5 | Affinity granted to parent lineage |

### Key Features

#### Cost Reduction

```csharp
public override void OnSummon(PlayerController player)
{
    // Apply scion-specific cost reduction
    tetherCostPerSecond *= scionAffinityBonus; // 0.8x = 20% reduction
    
    base.OnSummon(player);
}
```

#### Lineage Affinity Bonus

```csharp
public override void OnTetherMaintained(float deltaTime)
{
    base.OnTetherMaintained(deltaTime);
    
    // Grant bonus affinity to parent lineage
    string parentId = GetParentEntityId();
    if (!string.IsNullOrEmpty(parentId))
    {
        // 50% of gained affinity goes to parent
        AffinitySystem.Instance.AddTetherAffinity(
            parentId, 
            deltaTime * parentAffinityBonus, // 0.5x
            isTemperamentSatisfied
        );
    }
}
```

#### Reduced Punishment

```csharp
protected void ApplyPunishment(float baseDamage)
{
    if (boundPlayer != null)
    {
        SetTemperamentSatisfied(false);
        float actualDamage = baseDamage * punishmentMultiplier; // 0.5x
        boundPlayer.TakeDamage(actualDamage);
    }
}
```

### Abstract Methods

Scions must implement:

```csharp
protected abstract void ApplyLocalEnvironmentalShift();
protected abstract void PerformTemperamentCheck();
```

---

## Ember Scion, The Spark

> *"A young flame, eager but forgiving. Train with Ember before facing the Combustion."*

### Profile

**Location:** `Assets/Scripts/Entities/Tiers/EmberScion.cs`

| Property | Value |
|----------|-------|
| Lineage | Ignis Mater |
| Element | Fire |
| Temperament | Aggressive (mild) |
| Base Cost | 5 HP/sec → 4 HP/sec (with bonus) |
| Rampant Behavior | Chaotic |
| Rampant Duration | 15s |
| Rampant Damage | 10 |

### Temperament

Ember Scion has a more lenient aggressive temperament:

| Setting | Value | Comparison to Ignis |
|---------|-------|---------------------|
| `hesitationThreshold` | 5.0s | vs 3.0s |
| `basePunishmentDamage` | 3.0 | vs 5.0 (×0.5 = 1.5) |
| `temperamentTolerance` | 5.0s | Grace period before checks |

```csharp
protected override void PerformTemperamentCheck()
{
    // Only check periodically
    if (Time.time - lastTemperamentCheckTime < temperamentTolerance)
        return;
    
    lastTemperamentCheckTime = Time.time;
    
    if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
    {
        ApplyPunishment(basePunishmentDamage);
    }
    else
    {
        ApproveTemperament();
    }
}
```

### Environmental Shift

```csharp
protected override void ApplyLocalEnvironmentalShift()
{
    // Subtle orange glow (local effect)
    RenderSettings.ambientLight = Color.Lerp(
        RenderSettings.ambientLight, 
        new Color(1f, 0.5f, 0.2f), 
        0.3f
    );
}
```

- Warm orange glow (not full red)
- Blends with existing lighting
- Local area effect only

### Configuration

```csharp
void Start()
{
    entityName = "Ember, The Spark";
    entityId = "EmberScion";
    parentLineage = "Ignis Mater";
    tetherCostPerSecond = 5.0f;
    
    rampantBehavior = RampantBehavior.Chaotic;
    rampantDuration = 15f;
    rampantDamage = 10f;
    
    ConfigureRampantState();
}
```

### Strategy

1. Attack every 4-5 seconds (more lenient than Ignis)
2. Use to build Ignis Mater affinity safely
3. Short rampant duration if broken
4. Reduced punishment for mistakes

---

## Creating New Scions

### Template

```csharp
using UnityEngine;

public class NewScion : Scion
{
    [Header("Scion Settings")]
    public float myThreshold = 5.0f;
    public float myPunishment = 3.0f;
    public Color localColor = Color.cyan;
    
    private float lastCheckTime;
    
    void Start()
    {
        entityName = "NewScion, The Something";
        entityId = "NewScion";
        parentLineage = "AquaPater"; // or other parent
        tetherCostPerSecond = 5.0f;
        
        rampantBehavior = RampantBehavior.Chaotic;
        rampantDuration = 15f;
        rampantDamage = 10f;
        
        ConfigureRampantState();
    }
    
    protected override void ApplyLocalEnvironmentalShift()
    {
        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight, 
            localColor, 
            0.3f
        );
    }
    
    protected override void PerformTemperamentCheck()
    {
        if (Time.time - lastCheckTime < temperamentTolerance)
            return;
        
        lastCheckTime = Time.time;
        
        // Your temperament logic here
        bool conditionMet = CheckYourCondition();
        
        if (!conditionMet)
        {
            ApplyPunishment(myPunishment);
        }
        else
        {
            ApproveTemperament();
        }
    }
}
```

### Planned Scions

| Name | Lineage | Temperament |
|------|---------|-------------|
| Wave Scion | Aqua Pater | Passive (mild) |
| Stone Scion | Terra Mater | Rhythmic (mild) |

---

## Comparison: Scion vs Parent

| Feature | Scion | Parent |
|---------|-------|--------|
| **Base Cost** | 5/sec | 7-10/sec |
| **Cost Modifier** | 0.8x built-in | None |
| **Temperament** | Lenient | Strict |
| **Grace Period** | 5 seconds | None |
| **Punishment** | 0.5x | Full |
| **Environmental** | Local (15m) | Global |
| **Rampant Duration** | 15 seconds | 35-45 seconds |
| **Rampant Damage** | 10 | 15-20 |
| **Lineage Bonus** | Grants 50% to parent | N/A |

## Affinity Progression

Training with Scions builds toward Parent mastery:

```
Train with Ember Scion
├── 100% affinity to Ember
└── 50% affinity to Ignis Mater

After 10 successful tethers with Ember:
├── Ember: ~50% affinity
└── Ignis Mater: ~25% affinity

Benefits when tethering Ignis Mater:
├── Reduced cost (Acquainted level)
└── Safer learning curve
```

## Best Practices

1. **Start with Scions** when learning a new lineage
2. **Master the temperament** at a slower pace
3. **Build parent affinity** before attempting Parents
4. **Use shorter tethers** to learn safely
5. **Graduate to Parents** once comfortable

---

[← Parents](Parents) | [Heirs →](Heirs)
