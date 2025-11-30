# API Reference

Complete API reference for Lineage of the Arcane systems.

## Table of Contents

- [Core Systems](#core-systems)
- [Entity Classes](#entity-classes)
- [Player Systems](#player-systems)
- [UI Components](#ui-components)
- [Audio Systems](#audio-systems)
- [Effects](#effects)
- [Multiplayer](#multiplayer)

---

## Core Systems

### AffinitySystem

**Singleton** managing player-entity relationships.

```csharp
public class AffinitySystem : MonoBehaviour
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `AffinitySystem` | Singleton accessor |
| `affinityGainRate` | `float` | Base gain per second (0.5) |
| `betrayalPenalty` | `float` | Affinity lost on betrayal (15) |
| `temperamentBonus` | `float` | Bonus when meeting requirements (0.2) |
| `cleanSeverBonus` | `float` | Bonus for clean sever (5) |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetAffinityData(entityId)` | `AffinityData` | Get full affinity record |
| `GetAffinityLevel(entityId)` | `AffinityLevel` | Get current level |
| `GetAffinityPercentage(entityId)` | `float` | Get percentage (0-100) |
| `GetProgressToNextLevel(entityId)` | `float` | Progress to next level (0-1) |
| `GetTetherCostMultiplier(entityId)` | `float` | Cost modifier for level |
| `AddTetherAffinity(entityId, deltaTime, satisfied)` | `void` | Add continuous affinity |
| `OnSuccessfulTether(entityId)` | `void` | Record clean sever |
| `OnSuccessfulTetherWithBonus(entityId, bonus)` | `void` | Clean sever with extra bonus |
| `OnTetherBetrayal(entityId)` | `void` | Record betrayal |
| `OnTetherBetrayalWithPenalty(entityId, penalty)` | `void` | Betrayal with custom penalty |
| `IsAbilityUnlocked(entityId)` | `bool` | Check if ability unlocked |
| `ResetAffinity(entityId)` | `void` | Reset affinity data |
| `GetAffinitySummary(entityId)` | `string` | Debug summary string |

#### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAffinityLevelChanged` | `Action<string, AffinityLevel>` | Level changed |
| `OnAffinityGained` | `Action<string, float>` | Affinity gained |
| `OnAffinityLost` | `Action<string, float>` | Affinity lost |
| `OnAbilityUnlocked` | `Action<string>` | Ability unlocked |

---

### TetherSystem

Manages tether connections and health drain.

```csharp
public class TetherSystem : MonoBehaviour
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `activeSummon` | `MagicParent` | Currently tethered entity |
| `player` | `PlayerController` | Player reference |
| `isTethered` | `bool` | Whether tether is active |
| `tetherColor` | `Color` | Visual color |
| `tetherWidth` | `float` | Visual width |
| `safeSeverThreshold` | `float` | Min health % for safe sever |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `InitiateTether(summon)` | `void` | Start tether with entity |
| `SeverTether()` | `void` | End tether (auto-determines type) |
| `SeverTetherCleanly()` | `void` | Force clean sever |
| `GetTetherSessionInfo()` | `string` | Session info string |
| `GetCurrentTetherCost()` | `float` | Current cost/sec |
| `GetSessionDuration()` | `float` | Session duration in seconds |

---

### RampantState

Handles hostile AI when tethers break.

```csharp
public class RampantState : MonoBehaviour
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `behavior` | `RampantBehavior` | Behavior type |
| `rampantDuration` | `float` | Duration in seconds |
| `attackInterval` | `float` | Time between attacks |
| `damagePerAttack` | `float` | Damage per attack |
| `detectionRadius` | `float` | Target detection range |
| `moveSpeed` | `float` | Movement speed |
| `isRampant` | `bool` | Whether rampant is active |
| `rampantStartTime` | `float` | When rampant began |
| `lastKnownPlayerPosition` | `Transform` | Last known player position |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `EnterRampantState(playerPos)` | `void` | Enter rampant state |
| `ExitRampantState()` | `void` | Exit rampant state |
| `AttemptRebind(player)` | `bool` | Try to rebind entity |

---

## Entity Classes

### MagicParent (Abstract)

Base class for all magical entities.

```csharp
[RequireComponent(typeof(RampantState))]
public abstract class MagicParent : MonoBehaviour
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `entityName` | `string` | Display name |
| `entityId` | `string` | Unique identifier |
| `tetherCostPerSecond` | `float` | Base tether cost |
| `defianceLevel` | `float` | Chance to ignore orders |
| `rampantBehavior` | `RampantBehavior` | Rampant behavior type |
| `rampantDuration` | `float` | Rampant duration |
| `rampantDamage` | `float` | Rampant damage |

#### Abstract Methods

| Method | Description |
|--------|-------------|
| `ApplyEnvironmentalShift()` | Apply world changes |
| `CheckTemperament()` | Validate player behavior |

#### Virtual Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `OnSummon(player)` | `void` | Called when tethered |
| `OnTetherMaintained(deltaTime)` | `void` | Called each frame |
| `OnTetherBroken()` | `void` | Called when tether breaks |
| `OnTetherSeveredCleanly()` | `void` | Called on clean sever |
| `ActivateSpecialAbility()` | `void` | Activate special ability |
| `OnSpecialAbilityAvailable()` | `void` | Called when ability unlocks |

#### Utility Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetModifiedTetherCost()` | `float` | Cost after affinity |
| `GetAffinityInfo()` | `string` | Affinity summary |
| `GetAffinityLevel()` | `AffinityLevel` | Current level |
| `IsRampant()` | `bool` | Rampant state check |
| `GetBoundPlayer()` | `PlayerController` | Bound player |

---

### Parent Entities

#### IgnisMater

| Method | Returns | Description |
|--------|---------|-------------|
| `IsInfernoActive()` | `bool` | Ability active check |
| `GetAbilityCooldownRemaining()` | `float` | Remaining cooldown |

#### AquaPater

| Method | Returns | Description |
|--------|---------|-------------|
| `OnDismiss()` | `void` | Clean dismissal |
| `IsSanctuaryActive()` | `bool` | Ability active check |
| `GetAbilityCooldownRemaining()` | `float` | Remaining cooldown |

#### TerraMater

| Method | Returns | Description |
|--------|---------|-------------|
| `OnDismiss()` | `void` | Clean dismissal |
| `GetRhythmStreak()` | `int` | Current rhythm streak |
| `ApplyRhythmBonus()` | `void` | Apply streak bonus |
| `IsBulwarkActive()` | `bool` | Ability active check |
| `AbsorbDamage(damage)` | `float` | Absorb damage, return passthrough |
| `GetAbilityCooldownRemaining()` | `float` | Remaining cooldown |
| `GetBarrierHealth()` | `float` | Current barrier HP |

---

### Scion Base Class

```csharp
public abstract class Scion : MagicParent
```

| Property | Type | Description |
|----------|------|-------------|
| `parentLineage` | `string` | Parent entity name |
| `localEffectRadius` | `float` | Effect radius |
| `scionAffinityBonus` | `float` | Cost reduction (0.8) |
| `temperamentTolerance` | `float` | Grace period |
| `punishmentMultiplier` | `float` | Reduced punishment (0.5) |
| `parentAffinityBonus` | `float` | Lineage bonus (0.5) |

---

### Heir Base Class

```csharp
public abstract class Heir : MagicParent
```

| Property | Type | Description |
|----------|------|-------------|
| `ancestralLine` | `string` | Lineage name |
| `effectRange` | `float` | Minimal effect range |
| `heirAffinityBonus` | `float` | Cost reduction (0.5) |
| `temperamentGracePeriod` | `float` | Long grace period |
| `punishmentMultiplier` | `float` | Minimal punishment (0.2) |
| `isForgiving` | `bool` | Whether to punish |
| `bondStrength` | `float` | Current bond |
| `maxBondStrength` | `float` | Max bond |
| `bondGrowthRate` | `float` | Growth per second |
| `ancestralAffinityBonus` | `float` | Lineage bonus (0.3) |
| `affinityGainMultiplier` | `float` | Gain multiplier (1.5) |

| Method | Returns | Description |
|--------|---------|-------------|
| `GetBondPercentage()` | `float` | Bond as percentage |
| `CanEvolve()` | `bool` | Max bond reached |

---

## Player Systems

### PlayerController

```csharp
public class PlayerController : MonoBehaviour
```

| Property | Type | Description |
|----------|------|-------------|
| `maxHealth` | `float` | Maximum health (100) |
| `currentHealth` | `float` | Current health |
| `maxSanity` | `float` | Maximum sanity (100) |
| `currentSanity` | `float` | Current sanity |
| `lastAttackTime` | `float` | Time of last attack |
| `moveSpeed` | `float` | Movement speed |

| Method | Returns | Description |
|--------|---------|-------------|
| `PerformAttack()` | `void` | Execute attack |
| `TakeDamage(damage)` | `void` | Apply damage |
| `DrainSanity(amount)` | `void` | Drain sanity |

---

## UI Components

### HealthBarUI

| Method | Returns | Description |
|--------|---------|-------------|
| `SetPlayer(player)` | `void` | Set player reference |
| `ResetBurnedHealth()` | `void` | Clear burned health |
| `FlashDamage()` | `void` | Flash damage effect |

### SanityIndicatorUI

| Method | Returns | Description |
|--------|---------|-------------|
| `SetPlayer(player)` | `void` | Set player reference |
| `IsLowSanity()` | `bool` | Low sanity check |
| `IsCriticalSanity()` | `bool` | Critical sanity check |
| `FlashSanityDrain()` | `void` | Flash effect |

### TetherDisplayUI

| Method | Returns | Description |
|--------|---------|-------------|
| `SetTetherSystem(tether)` | `void` | Set tether reference |
| `SetTemperamentStatus(status)` | `void` | Set temperament display |
| `GetTetherStrength()` | `float` | Current strength |
| `IsTethered()` | `bool` | Tether active check |
| `FlashTetherWarning()` | `void` | Warning flash |

### AffinityDisplayUI

| Method | Returns | Description |
|--------|---------|-------------|
| `UpdateDisplay(entity)` | `void` | Update with entity |
| `ClearDisplay()` | `void` | Clear display |

---

## Audio Systems

### AudioManager (Singleton)

| Method | Returns | Description |
|--------|---------|-------------|
| `PlayTetherForm()` | `void` | Tether form sound |
| `PlayTetherBreak()` | `void` | Tether break sound |
| `StartTetherPulse()` | `void` | Start pulse loop |
| `StopTetherPulse()` | `void` | Stop pulse loop |
| `PlayViolationWarning()` | `void` | Temperament warning |
| `PlayPunishment()` | `void` | Punishment sound |
| `PlayRampantStart()` | `void` | Rampant start |
| `PlayRampantEnd()` | `void` | Rampant end |
| `PlayPlayerDamage()` | `void` | Player damage |
| `PlayPlayerDeath()` | `void` | Player death |
| `SetParentAmbient(profile)` | `void` | Set ambient sound |
| `StopAmbient()` | `void` | Stop ambient |
| `UpdateSanityAudio(percent)` | `void` | Update sanity effects |
| `PlaySFX(clip, volume)` | `void` | Play one-shot |
| `SetMasterVolume(volume)` | `void` | Set master volume |
| `SetMusicVolume(volume)` | `void` | Set music volume |
| `SetSFXVolume(volume)` | `void` | Set SFX volume |
| `UpdateVolumes()` | `void` | Apply volume changes |

### ParentAudioProfile (ScriptableObject)

| Property | Type | Description |
|----------|------|-------------|
| `parentName` | `string` | Parent name |
| `description` | `string` | Description |
| `ambientSound` | `AudioClip` | Ambient loop |
| `summonSound` | `AudioClip` | Summon sound |
| `tetherBreakSound` | `AudioClip` | Break sound |
| `rampantSound` | `AudioClip` | Rampant sound |
| `temperamentWarningSound` | `AudioClip` | Warning sound |
| `punishmentSound` | `AudioClip` | Punishment sound |
| `pleasedSound` | `AudioClip` | Pleased sound |

| Method | Returns | Description |
|--------|---------|-------------|
| `GetRandomizedPitch()` | `float` | Randomized pitch |

---

## Effects

### TetherVisualEffect

| Method | Returns | Description |
|--------|---------|-------------|
| `ShowViolation()` | `void` | Show violation color |
| `FlashTether(color, duration)` | `void` | Flash effect |
| `GetTetherLength()` | `float` | Current tether length |

---

## Multiplayer

### CustodyBattle

| Method | Returns | Description |
|--------|---------|-------------|
| `StartCustodyBattle(p1, p2, parent)` | `void` | Start battle |
| `ReportTemperamentAction(player, score)` | `void` | Report action |
| `GetPlayerFavor(player)` | `float` | Get player's favor % |
| `WithdrawFromBattle(player)` | `void` | Withdraw |

---

[← Back to Home](Home)
