# Parent Entities (Tier 0)

Parents are the most powerful magical entities in Lineage of the Arcane. They are ancient Progenitors with unique temperaments, global environmental effects, and powerful special abilities.

## Overview

| Entity | Element | Temperament | Cost/sec | Special Ability |
|--------|---------|-------------|----------|-----------------|
| Ignis Mater | Fire | Aggressive | 10 | Inferno Embrace |
| Aqua Pater | Water | Passive | 8 | Tidal Sanctuary |
| Terra Mater | Earth | Rhythmic | 7 | Earthen Bulwark |

---

## Ignis Mater, The Combustion

> *"The Fire Mother demands your fury. Hesitate, and she will consume you."*

### Profile

**Location:** `Assets/Scripts/Entities/IgnisMater.cs`

| Property | Value |
|----------|-------|
| Element | Fire |
| Temperament | Aggressive |
| Base Cost | 10 HP/sec |
| Rampant Behavior | Aggressive |
| Rampant Duration | 45s |
| Rampant Damage | 20 |

### Temperament: Aggressive

Ignis Mater demands constant combat. Players must attack regularly.

| Setting | Value | Description |
|---------|-------|-------------|
| `hesitationThreshold` | 3.0s | Max time without attacking |
| `punishmentDamage` | 5.0 | Damage per violation |

```csharp
public override void CheckTemperament()
{
    if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
    {
        SetTemperamentSatisfied(false);
        boundPlayer.TakeDamage(punishmentDamage);
        Debug.Log("Ignis burns you for your hesitation!");
    }
    else
    {
        SetTemperamentSatisfied(true);
    }
}
```

### Environmental Shift

```csharp
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = Color.red;
    Debug.Log("The world heats up. Ignis is watching.");
}
```

- Ambient light turns red
- Intensifies to 2x red when rampant
- Further intensifies to 3x during Inferno Embrace

### Special Ability: Inferno Embrace

**Unlock:** Reach Ascended affinity (100%)

| Property | Value |
|----------|-------|
| Duration | 5 seconds |
| Damage | 10 HP/sec in radius |
| Radius | 8 meters |
| Cooldown | 30 seconds |

**Effects:**
- Temporary invulnerability
- AoE damage to nearby enemies
- Intense visual effects

```csharp
public override void ActivateSpecialAbility()
{
    if (!hasSpecialAbility) return;
    if (Time.time - lastAbilityUseTime < abilityCooldown) return;
    
    isInfernoActive = true;
    infernoStartTime = Time.time;
    lastAbilityUseTime = Time.time;
    
    RenderSettings.ambientLight = Color.red * 3f;
}
```

### Strategy

1. Attack every 2-3 seconds to satisfy temperament
2. Keep health above 50% for sustained tethering
3. Build to Ascended for Inferno Embrace
4. Use ability strategically—30s cooldown

---

## Aqua Pater, The Depths

> *"The Water Father seeks stillness. Violence disturbs his waters."*

### Profile

**Location:** `Assets/Scripts/Entities/AquaPater.cs`

| Property | Value |
|----------|-------|
| Element | Water |
| Temperament | Passive |
| Base Cost | 8 HP/sec |
| Rampant Behavior | Chaotic |
| Rampant Duration | 35s |
| Rampant Damage | 15 |

### Temperament: Passive

Aqua Pater demands pacifism. Players must not attack.

| Setting | Value | Description |
|---------|-------|-------------|
| `aggressionThreshold` | 2.0s | Cooldown after attacking |
| `punishmentDamage` | 4.0 | Damage per second during aggression |

```csharp
public override void CheckTemperament()
{
    // Punish if player attacked recently
    if (Time.time - boundPlayer.lastAttackTime < aggressionThreshold)
    {
        SetTemperamentSatisfied(false);
        if (Time.time - lastAggressionTime > 1.0f)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            lastAggressionTime = Time.time;
        }
    }
    else
    {
        SetTemperamentSatisfied(true);
    }
}
```

### Environmental Shift

```csharp
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = new Color(0.2f, 0.4f, 0.8f);
    
    // Slow player movement
    originalMoveSpeed = boundPlayer.moveSpeed;
    boundPlayer.moveSpeed *= 0.6f; // 40% slower
}
```

- Deep blue ambient light
- Player movement slowed by 40%
- Movement restored when dismissed

### Special Ability: Tidal Sanctuary

**Unlock:** Reach Ascended affinity (100%)

| Property | Value |
|----------|-------|
| Duration | 8 seconds |
| Healing | 5 HP/sec |
| Radius | 6 meters |
| Cooldown | 45 seconds |

**Effects:**
- Creates healing zone at player position
- Heals player while in zone
- Calming blue visual effects

```csharp
private void UpdateSanctuaryAbility()
{
    if (boundPlayer != null)
    {
        float distance = Vector3.Distance(
            boundPlayer.transform.position, 
            sanctuaryPosition
        );
        if (distance <= sanctuaryRadius)
        {
            float healAmount = healingPerSecond * Time.deltaTime;
            boundPlayer.currentHealth = Mathf.Min(
                boundPlayer.currentHealth + healAmount,
                boundPlayer.maxHealth
            );
        }
    }
}
```

### Strategy

1. Do NOT attack while tethered
2. Wait 2+ seconds after combat before tethering
3. Use for defensive/exploration situations
4. Tidal Sanctuary provides excellent sustain

---

## Terra Mater, The Foundation

> *"The Earth Mother feels your rhythm. Break the pattern, and she will break you."*

### Profile

**Location:** `Assets/Scripts/Entities/TerraMater.cs`

| Property | Value |
|----------|-------|
| Element | Earth |
| Temperament | Rhythmic |
| Base Cost | 7 HP/sec |
| Rampant Behavior | Destructive |
| Rampant Duration | 40s |
| Rampant Damage | 18 |

### Temperament: Rhythmic

Terra Mater demands consistent, patterned actions.

| Setting | Value | Description |
|---------|-------|-------------|
| `rhythmWindow` | 3.0s | Expected interval between actions |
| `rhythmTolerance` | 0.5s | Acceptable deviation |
| `punishmentDamage` | 6.0 | Damage per violation |

```csharp
public override void CheckTemperament()
{
    if (boundPlayer.lastAttackTime > lastActionTime)
    {
        float interval = boundPlayer.lastAttackTime - lastActionTime;
        float deviation = Mathf.Abs(interval - rhythmWindow);
        
        if (hasStartedRhythm)
        {
            if (deviation <= rhythmTolerance)
            {
                rhythmStreak++;
                SetTemperamentSatisfied(true);
            }
            else
            {
                SetTemperamentSatisfied(false);
                boundPlayer.TakeDamage(punishmentDamage);
                rhythmStreak = 0;
            }
        }
        else
        {
            hasStartedRhythm = true;
        }
        
        lastActionTime = boundPlayer.lastAttackTime;
    }
    
    // Punish silence
    if (hasStartedRhythm && Time.time - lastActionTime > rhythmWindow + rhythmTolerance * 2f)
    {
        SetTemperamentSatisfied(false);
        boundPlayer.TakeDamage(punishmentDamage);
        hasStartedRhythm = false;
        rhythmStreak = 0;
    }
}
```

### Environmental Shift

```csharp
protected override void ApplyEnvironmentalShift()
{
    RenderSettings.ambientLight = new Color(0.6f, 0.4f, 0.2f);
    
    // Increase gravity
    originalGravity = Physics.gravity.y;
    Physics.gravity = new Vector3(0, originalGravity * 1.5f, 0);
}
```

- Earthy brown ambient light
- Gravity increased by 50%
- Extreme gravity (2x) during rampant state

### Special Ability: Earthen Bulwark

**Unlock:** Reach Ascended affinity (100%)

| Property | Value |
|----------|-------|
| Duration | 10 seconds |
| Shield | 50 damage absorbed |
| Cooldown | 60 seconds |

**Effects:**
- Creates protective barrier
- Absorbs incoming damage
- Breaks when depleted or duration ends

```csharp
public float AbsorbDamage(float damage)
{
    if (!isBulwarkActive) return damage;
    
    if (damage <= currentBarrierHealth)
    {
        currentBarrierHealth -= damage;
        return 0f; // All absorbed
    }
    else
    {
        float passthrough = damage - currentBarrierHealth;
        currentBarrierHealth = 0;
        EndBulwarkAbility();
        return passthrough; // Partial absorption
    }
}
```

### Rhythm Bonus

Maintaining rhythm provides additional benefits:

```csharp
public void ApplyRhythmBonus()
{
    if (rhythmStreak >= 5)
    {
        // Reduce tether cost for good rhythm
        float reduction = tetherCostPerSecond * 0.1f * (rhythmStreak / 5);
    }
}
```

### Strategy

1. Attack every 3 seconds (±0.5s tolerance)
2. Use a mental count or audio cue
3. Build rhythm streaks for cost reduction
4. Earthen Bulwark provides excellent defense

---

## Comparison Table

| Feature | Ignis Mater | Aqua Pater | Terra Mater |
|---------|-------------|------------|-------------|
| **Temperament** | Aggressive | Passive | Rhythmic |
| **Requirement** | Attack often | Don't attack | Attack rhythmically |
| **Base Cost** | 10/sec | 8/sec | 7/sec |
| **Punishment** | 5 damage | 4 damage/sec | 6 damage |
| **Rampant Type** | Aggressive | Chaotic | Destructive |
| **Rampant Duration** | 45s | 35s | 40s |
| **Ability** | Invuln + AoE | Healing zone | Damage shield |
| **Ability Duration** | 5s | 8s | 10s |
| **Ability Cooldown** | 30s | 45s | 60s |

## Best Practices

1. **Start with Heirs** to build affinity safely
2. **Graduate through Scions** for moderate challenge
3. **Attempt Parents** only with built-up affinity
4. **Match playstyle** to temperament requirements
5. **Build to Ascended** for special abilities

---

[← Magic Parent](Magic-Parent) | [Scions →](Scions)
