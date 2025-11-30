# Heir Entities (Tier 2)

Heirs are the gentlest magical entities—perfect for new players or for safely building relationships with powerful lineages.

## Overview

Heirs are designed to be **forgiving** and **safe**:

- **Very low tether cost** (2 HP/sec base)
- **Minimal environmental effects** (5m radius)
- **Extremely forgiving temperament** (rarely punish)
- **Do NOT go rampant** (simply fade away)
- **Faster affinity gain** (1.5x rate)
- **Reduced betrayal penalty** (-5 vs -15)
- **Lineage affinity bonus** (30% to ancestor)

## Heir Base Class

**Location:** `Assets/Scripts/Entities/Tiers/Heir.cs`

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ancestralLine` | `string` | - | The lineage they belong to |
| `effectRange` | `float` | 5 | Very limited effect range |
| `heirAffinityBonus` | `float` | 0.5 | Tether cost multiplier (50% reduction) |
| `temperamentGracePeriod` | `float` | 10 | Long grace before any check |
| `punishmentMultiplier` | `float` | 0.2 | Very mild punishment (20%) |
| `isForgiving` | `bool` | true | Whether heir punishes at all |
| `bondStrength` | `float` | 0 | Grows with tethering |
| `maxBondStrength` | `float` | 100 | Maximum bond level |
| `bondGrowthRate` | `float` | 1 | Growth per second |
| `ancestralAffinityBonus` | `float` | 0.3 | Affinity to ancestral parent |
| `affinityGainMultiplier` | `float` | 1.5 | Faster affinity gain |

### Key Features

#### Significant Cost Reduction

```csharp
public override void OnSummon(PlayerController player)
{
    tetherCostPerSecond *= heirAffinityBonus; // 0.5x = 50% reduction
    base.OnSummon(player);
}
```

#### Faster Affinity Gain

```csharp
public override void OnTetherMaintained(float deltaTime)
{
    // Heirs give 1.5x affinity due to gentle nature
    float bonusTime = deltaTime * affinityGainMultiplier;
    AffinitySystem.Instance.AddTetherAffinity(entityId, bonusTime, isTemperamentSatisfied);
    
    // Also grant bonus to ancestral lineage
    string parentId = GetAncestralParentId();
    if (!string.IsNullOrEmpty(parentId))
    {
        AffinitySystem.Instance.AddTetherAffinity(
            parentId, 
            deltaTime * ancestralAffinityBonus, // 30%
            true
        );
    }
}
```

#### No Rampant State

```csharp
public override void OnTetherBroken()
{
    // Heirs do NOT go rampant - they simply fade away
    Debug.Log($"Heir {entityName} fades away with a sad expression...");
    
    // Reduced betrayal penalty
    AffinitySystem.Instance.OnTetherBetrayalWithPenalty(entityId, 5f);
    
    // Do NOT call base.OnTetherBroken() - no rampant state
}
```

#### Extra Clean Sever Bonus

```csharp
public override void OnTetherSeveredCleanly()
{
    // Extra affinity bonus for heirs
    AffinitySystem.Instance.OnSuccessfulTetherWithBonus(entityId, 3f);
    boundPlayer = null;
}
```

#### Forgiving Temperament

```csharp
public override void CheckTemperament()
{
    // Heirs are always temperament-satisfied by default
    SetTemperamentSatisfied(true);
    
    if (isForgiving)
    {
        PerformGentleTemperamentCheck(); // Usually does nothing
    }
}
```

### Bond Growth System

Heirs have a unique **Bond** system that grows over time:

```csharp
protected void GrowBond()
{
    bondStrength += bondGrowthRate * Time.deltaTime;
    bondStrength = Mathf.Min(bondStrength, maxBondStrength);
}

public float GetBondPercentage()
{
    return (bondStrength / maxBondStrength) * 100f;
}

public bool CanEvolve()
{
    return bondStrength >= maxBondStrength;
}
```

---

## Candlelight Heir, The Gentle Flame

> *"A tiny flame that asks only for warmth. Candlelight will never hurt you."*

### Profile

**Location:** `Assets/Scripts/Entities/Tiers/CandlelightHeir.cs`

| Property | Value |
|----------|-------|
| Lineage | Ignis (Fire) |
| Element | Fire |
| Temperament | None (forgiving) |
| Base Cost | 2 HP/sec → 1 HP/sec (with bonus) |
| Bond Growth | 2/sec (fast) |
| Rampant | None (fades away) |

### Configuration

```csharp
void Start()
{
    entityName = "Candlelight, The Gentle Flame";
    entityId = "CandlelightHeir";
    ancestralLine = "Ignis";
    tetherCostPerSecond = 2.0f;
    
    // Heirs don't go rampant
    rampantBehavior = RampantBehavior.Chaotic;
    rampantDuration = 5f;
    rampantDamage = 2f;
    
    isForgiving = true;
    bondGrowthRate = 2f; // Fast bonding
    
    ConfigureRampantState();
}
```

### Environmental Shift

```csharp
protected override void ApplyMinimalEnvironmentalShift()
{
    CreateOrUpdateLight();
}

void CreateOrUpdateLight()
{
    if (pointLight == null)
    {
        pointLight = gameObject.AddComponent<Light>();
    }
    
    pointLight.type = LightType.Point;
    pointLight.color = new Color(1f, 0.8f, 0.4f); // Warm glow
    pointLight.range = 3f; // Small radius
    pointLight.intensity = 1f;
}
```

- Creates a soft point light
- Warm candle-like color
- Gentle flickering effect
- Very limited range (3m)

### Flickering Effect

```csharp
void UpdateFlicker()
{
    if (pointLight != null)
    {
        float flicker = 1f + Mathf.Sin(Time.time * flickerSpeed) * 0.1f;
        pointLight.intensity = flicker;
    }
}
```

### Temperament (None)

```csharp
protected override void PerformTemperamentCheck()
{
    // Candlelight has no requirements
    // Just hopes for some interaction
    Debug.Log("Candlelight flickers gently, hoping for some interaction.");
}

protected override void PerformGentleTemperamentCheck()
{
    // Very forgiving - no actual checks
}
```

### Solace Ability

At high bond strength, Candlelight can provide minor healing:

```csharp
public void ProvideSolace()
{
    if (boundPlayer != null && bondStrength > 50f)
    {
        float healAmount = bondStrength / maxBondStrength * 2f;
        boundPlayer.currentHealth = Mathf.Min(
            boundPlayer.currentHealth + healAmount,
            boundPlayer.maxHealth
        );
    }
}
```

### Strategy

1. Use Candlelight for safe, risk-free tethering
2. Build bond strength over time
3. Gain affinity with Ignis Mater passively
4. Prepare for Ember Scion or Ignis Mater

---

## Creating New Heirs

### Template

```csharp
using UnityEngine;

public class NewHeir : Heir
{
    [Header("Heir Settings")]
    public Color glowColor = Color.cyan;
    public float glowRange = 3f;
    
    private Light pointLight;
    private float lastCheckTime;
    
    void Start()
    {
        entityName = "NewHeir, The Gentle";
        entityId = "NewHeir";
        ancestralLine = "Aqua"; // or "Ignis", "Terra"
        tetherCostPerSecond = 2.0f;
        
        rampantBehavior = RampantBehavior.Chaotic;
        rampantDuration = 5f;
        rampantDamage = 2f;
        
        isForgiving = true;
        bondGrowthRate = 2f;
        
        ConfigureRampantState();
    }
    
    protected override void ApplyMinimalEnvironmentalShift()
    {
        // Create subtle visual effect
        if (pointLight == null)
        {
            pointLight = gameObject.AddComponent<Light>();
        }
        pointLight.type = LightType.Point;
        pointLight.color = glowColor;
        pointLight.range = glowRange;
    }
    
    protected override void PerformTemperamentCheck()
    {
        // Heirs are forgiving - no punishment
        if (Time.time - lastCheckTime < temperamentGracePeriod)
            return;
        
        lastCheckTime = Time.time;
        Debug.Log("Heir watches patiently...");
    }
}
```

### Planned Heirs

| Name | Lineage | Effect |
|------|---------|--------|
| Dewdrop Heir | Aqua Pater | Gentle mist, calming |
| Pebble Heir | Terra Mater | Slight earth tremor |

---

## Comparison: Heir vs Scion vs Parent

| Feature | Heir | Scion | Parent |
|---------|------|-------|--------|
| **Base Cost** | 2/sec | 5/sec | 7-10/sec |
| **Cost Modifier** | 0.5x | 0.8x | None |
| **Temperament** | None/Forgiving | Lenient | Strict |
| **Grace Period** | 10 seconds | 5 seconds | None |
| **Punishment** | 0.2x (if any) | 0.5x | Full |
| **Environmental** | Minimal (5m) | Local (15m) | Global |
| **Rampant** | None (fades) | 15 seconds | 35-45 seconds |
| **Affinity Gain** | 1.5x | 1.0x | 1.0x |
| **Betrayal Penalty** | -5 | -15 | -15 |
| **Lineage Bonus** | 30% | 50% | N/A |
| **Bond System** | Yes | No | No |

## Affinity Progression with Heirs

Training with Heirs provides the safest path to powerful Parents:

```
Train with Candlelight Heir
├── 150% affinity rate to Candlelight (1.5x)
└── 30% affinity to Ignis Mater

After 20 successful tethers with Candlelight:
├── Candlelight: ~80% affinity (fast!)
├── Ignis Mater: ~15% affinity
└── Reduced cost for future Ignis tethers

Progression path:
Candlelight Heir → Ember Scion → Ignis Mater
```

## Why Use Heirs?

1. **Learning** - Understand mechanics without risk
2. **Safety** - No rampant state, minimal punishment
3. **Efficiency** - 1.5x affinity gain rate
4. **Foundation** - Build lineage affinity safely
5. **Recovery** - Train after hostile relationship

## Best Practices

1. **Start every lineage** with its Heir
2. **Build bond strength** for extra benefits
3. **Use longer tethers** (low cost allows it)
4. **Graduate to Scions** after building affinity
5. **Return to Heirs** if relationship becomes hostile

---

[← Scions](Scions) | [Player Controller →](Player-Controller)
