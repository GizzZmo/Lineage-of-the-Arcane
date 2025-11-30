using UnityEngine;

/// <summary>
/// The Blueprint for all "Parents of Magic" - the abstract base class for all magical entities.
/// 
/// <para>
/// In Lineage of the Arcane, magic is not a resource to be spent—it's a relationship to be maintained.
/// Each magical entity (called a "Parent") has its own personality, demands, and risks.
/// Summoning one creates a "Tether" that drains the player's life force.
/// </para>
/// 
/// <para><b>Entity Hierarchy:</b></para>
/// <list type="bullet">
///   <item><description>Tier 0: Parents (Progenitors) - Most powerful, highest cost, global effects</description></item>
///   <item><description>Tier 1: Scions - Moderate power, local effects, simpler temperament</description></item>
///   <item><description>Tier 2: Heirs - Lowest power, minimal effects, very forgiving</description></item>
/// </list>
/// 
/// <para><b>Key Systems:</b></para>
/// <list type="bullet">
///   <item><description>Tether System - Health drain while connected</description></item>
///   <item><description>Temperament System - Behavioral requirements (Aggressive/Passive/Rhythmic)</description></item>
///   <item><description>Affinity System - Relationship tracking with cost modifiers</description></item>
///   <item><description>Rampant System - Hostile AI when tether breaks</description></item>
/// </list>
/// 
/// <para>RequireComponent ensures the RampantState is always present on the GameObject.</para>
/// </summary>
/// <remarks>
/// To create a new entity:
/// 1. Create a class inheriting from MagicParent (or Scion/Heir for lower tiers)
/// 2. Implement ApplyEnvironmentalShift() for world effects
/// 3. Implement CheckTemperament() for behavioral requirements
/// 4. Configure properties in Start()
/// </remarks>
/// <seealso cref="Scion"/>
/// <seealso cref="Heir"/>
/// <seealso cref="TetherSystem"/>
/// <seealso cref="AffinitySystem"/>
[RequireComponent(typeof(RampantState))]
public abstract class MagicParent : MonoBehaviour
{
    #region Entity Configuration
    
    [Header("Entity Stats")]
    /// <summary>The display name of this magical entity (e.g., "Ignis Mater, The Combustion").</summary>
    public string entityName;
    
    /// <summary>
    /// Base health drain per second while tethered to this entity.
    /// Modified by affinity level: Hostile (+50%) to Ascended (-50%).
    /// </summary>
    public float tetherCostPerSecond = 5.0f;
    
    /// <summary>
    /// Chance (0-100) that this entity will ignore player commands.
    /// Reserved for future implementation of entity autonomy.
    /// </summary>
    [Range(0, 100)] public float defianceLevel = 0f;

    #endregion
    
    #region Rampant Configuration
    
    [Header("Rampant Configuration")]
    /// <summary>
    /// Determines how this entity behaves when its tether breaks unexpectedly.
    /// Aggressive: Attacks nearest target. Chaotic: Random behavior.
    /// Vengeful: Targets the betrayer. Destructive: Destroys environment.
    /// </summary>
    public RampantBehavior rampantBehavior = RampantBehavior.Aggressive;
    
    /// <summary>Duration in seconds that the rampant state lasts before the entity becomes dormant.</summary>
    public float rampantDuration = 30f;
    
    /// <summary>Damage dealt per attack while in rampant state.</summary>
    public float rampantDamage = 15f;

    #endregion
    
    #region Affinity System
    
    [Header("Affinity System")]
    /// <summary>
    /// Unique identifier for tracking affinity with this entity type.
    /// Defaults to the class name if not explicitly set.
    /// </summary>
    [Tooltip("Unique identifier for affinity tracking")]
    public string entityId;
    
    /// <summary>
    /// Whether the player is currently meeting this entity's temperament requirements.
    /// When true, grants bonus affinity. When false, may trigger punishment.
    /// </summary>
    [Tooltip("Whether temperament requirements are currently being satisfied")]
    protected bool isTemperamentSatisfied = true;
    
    /// <summary>
    /// The calculated tether cost after applying affinity-based modifiers.
    /// Updated when a tether is initiated via UpdateTetherCostFromAffinity().
    /// </summary>
    [Tooltip("Current tether cost after affinity modifiers")]
    protected float modifiedTetherCost;
    
    /// <summary>
    /// Whether this entity's special ability is available (Ascended affinity required).
    /// </summary>
    [Tooltip("Whether the special ability is available")]
    protected bool hasSpecialAbility = false;

    #endregion
    
    #region Runtime References
    
    /// <summary>Reference to the currently tethered player. Null when not tethered.</summary>
    protected PlayerController boundPlayer;
    
    /// <summary>Reference to this entity's RampantState component for managing hostile behavior.</summary>
    protected RampantState rampantState;
    
    #endregion

    #region Unity Lifecycle
    
    /// <summary>
    /// Initializes the entity by setting up the RampantState component and entity ID.
    /// </summary>
    protected virtual void Awake()
    {
        // Get the RampantState component (guaranteed to exist via RequireComponent)
        rampantState = GetComponent<RampantState>();
        
        // Configure rampant state based on this parent's settings
        ConfigureRampantState();
        
        // Initialize entity ID if not set - defaults to class name for affinity tracking
        if (string.IsNullOrEmpty(entityId))
        {
            entityId = GetType().Name;
        }
    }
    
    #endregion
    
    #region Rampant State Management

    /// <summary>
    /// Configures the RampantState component with this Parent's specific settings.
    /// Called during Awake and can be called again in Start() after modifying rampant properties.
    /// </summary>
    protected virtual void ConfigureRampantState()
    {
        if (rampantState != null)
        {
            rampantState.behavior = rampantBehavior;
            rampantState.rampantDuration = rampantDuration;
            rampantState.damagePerAttack = rampantDamage;
        }
    }
    
    #endregion
    
    #region Tether Lifecycle

    /// <summary>
    /// Called when the player initiates a Tether connection with this entity.
    /// Sets up the bond, applies affinity modifiers, and triggers environmental effects.
    /// </summary>
    /// <param name="player">The player forming the tether.</param>
    /// <example>
    /// <code>
    /// // Typically called by TetherSystem.InitiateTether()
    /// magicParent.OnSummon(playerController);
    /// </code>
    /// </example>
    public virtual void OnSummon(PlayerController player)
    {
        boundPlayer = player;
        
        // Apply affinity-based cost modifier - higher affinity = lower cost
        UpdateTetherCostFromAffinity();
        
        // Check if special ability is unlocked (requires Ascended affinity level)
        hasSpecialAbility = AffinitySystem.Instance.IsAbilityUnlocked(entityId);
        
        Debug.Log($"{entityName} has entered the reality. The Tether is formed.");
        Debug.Log($"[AFFINITY] Current level: {AffinitySystem.Instance.GetAffinityLevel(entityId)}, " +
                  $"Cost modifier: {AffinitySystem.Instance.GetTetherCostMultiplier(entityId):F2}x");
        
        // Apply this entity's unique world-altering effects
        ApplyEnvironmentalShift();
        
        // Notify derived classes if special ability is available
        if (hasSpecialAbility)
        {
            OnSpecialAbilityAvailable();
        }
    }

    /// <summary>
    /// Recalculates the tether cost based on current affinity level.
    /// Cost modifiers range from 1.5x (Hostile) to 0.5x (Ascended).
    /// </summary>
    protected void UpdateTetherCostFromAffinity()
    {
        float costMultiplier = AffinitySystem.Instance.GetTetherCostMultiplier(entityId);
        modifiedTetherCost = tetherCostPerSecond * costMultiplier;
    }

    /// <summary>
    /// Gets the current tether cost per second after affinity modifiers are applied.
    /// Used by TetherSystem to calculate health drain.
    /// </summary>
    /// <returns>The modified tether cost, or base cost if modifiers haven't been applied.</returns>
    public float GetModifiedTetherCost()
    {
        return modifiedTetherCost > 0 ? modifiedTetherCost : tetherCostPerSecond;
    }

    /// <summary>
    /// Called every frame during active tether to accumulate affinity.
    /// Passes temperament satisfaction to determine bonus affinity gain.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame (typically Time.deltaTime).</param>
    /// <remarks>
    /// Base gain: 0.5 affinity/second
    /// Temperament bonus: +0.2 affinity/second when satisfied
    /// Override in derived classes for custom affinity behavior (e.g., Heirs gain 1.5x).
    /// </remarks>
    public virtual void OnTetherMaintained(float deltaTime)
    {
        AffinitySystem.Instance.AddTetherAffinity(entityId, deltaTime, isTemperamentSatisfied);
    }
    
    #endregion
    
    #region Special Abilities

    /// <summary>
    /// Called when the special ability becomes available through reaching Ascended affinity.
    /// Override to implement entity-specific notification or preparation.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void OnSpecialAbilityAvailable()
    /// {
    ///     Debug.Log("[IGNIS] Inferno Embrace is now available!");
    ///     // Play unlock sound/effect
    /// }
    /// </code>
    /// </example>
    protected virtual void OnSpecialAbilityAvailable()
    {
        Debug.Log($"{entityName}'s special ability is now available!");
    }

    /// <summary>
    /// Activates the entity's unique special ability (requires Ascended affinity).
    /// Override to implement entity-specific abilities like Inferno Embrace, Tidal Sanctuary, etc.
    /// </summary>
    /// <example>
    /// <code>
    /// public override void ActivateSpecialAbility()
    /// {
    ///     if (!hasSpecialAbility) return;
    ///     // Implement ability logic
    /// }
    /// </code>
    /// </example>
    public virtual void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning($"Special ability for {entityName} is not unlocked yet!");
            return;
        }
        
        Debug.Log($"{entityName} activates special ability!");
    }
    
    #endregion
    
    #region Abstract Methods - Must Be Implemented

    /// <summary>
    /// Applies this entity's unique environmental effects when summoned.
    /// Parents have global effects, Scions have local effects, Heirs have minimal effects.
    /// </summary>
    /// <example>
    /// <code>
    /// // Ignis Mater - Fire environmental shift
    /// protected override void ApplyEnvironmentalShift()
    /// {
    ///     RenderSettings.ambientLight = Color.red;
    ///     Debug.Log("The world heats up. Ignis is watching.");
    /// }
    /// </code>
    /// </example>
    protected abstract void ApplyEnvironmentalShift();

    /// <summary>
    /// Validates player behavior against this entity's temperament requirements.
    /// Called every frame while tethered. Must call SetTemperamentSatisfied() and apply punishment if needed.
    /// </summary>
    /// <remarks>
    /// Temperament types:
    /// - Aggressive (Ignis): Must attack frequently
    /// - Passive (Aqua): Must not attack
    /// - Rhythmic (Terra): Must act in patterns
    /// </remarks>
    /// <example>
    /// <code>
    /// public override void CheckTemperament()
    /// {
    ///     if (Time.time - boundPlayer.lastAttackTime > threshold)
    ///     {
    ///         SetTemperamentSatisfied(false);
    ///         boundPlayer.TakeDamage(punishmentDamage);
    ///     }
    ///     else
    ///     {
    ///         SetTemperamentSatisfied(true);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract void CheckTemperament();
    
    #endregion
    
    #region Tether Termination
    
    /// <summary>
    /// Called when the tether breaks unexpectedly (player health depleted).
    /// Records betrayal in affinity system and triggers rampant state.
    /// </summary>
    /// <remarks>
    /// This is the "bad" way to end a tether:
    /// - Costs -15 affinity (configurable)
    /// - Triggers rampant state (hostile AI)
    /// - May lead to Hostile relationship level after 3+ betrayals
    /// 
    /// Heirs override this to NOT enter rampant state.
    /// </remarks>
    public virtual void OnTetherBroken()
    {
        Debug.LogWarning($"{entityName} is now RAMPANT! The bond has been severed.");
        
        // Record the betrayal in the affinity system (-15 affinity by default)
        AffinitySystem.Instance.OnTetherBetrayal(entityId);
        
        // Trigger rampant state - entity becomes hostile
        if (rampantState != null && boundPlayer != null)
        {
            rampantState.EnterRampantState(boundPlayer.transform);
        }
        
        // Clear the bound player reference
        boundPlayer = null;
    }

    /// <summary>
    /// Called when the tether is manually severed cleanly by the player.
    /// This is the "good" way to end a tether - no rampant state, grants affinity bonus.
    /// </summary>
    /// <remarks>
    /// Clean sever requirements:
    /// - Player health must be above safeSeverThreshold (default 20%)
    /// - Grants +5 affinity bonus
    /// - No rampant state triggered
    /// 
    /// Heirs override this to grant extra bonus affinity.
    /// </remarks>
    public virtual void OnTetherSeveredCleanly()
    {
        Debug.Log($"{entityName} returns to the void peacefully.");
        
        // Record successful tether in the affinity system (+5 affinity)
        AffinitySystem.Instance.OnSuccessfulTether(entityId);
        
        // Clear the bound player reference
        boundPlayer = null;
    }
    
    #endregion
    
    #region Temperament Helpers

    /// <summary>
    /// Sets whether temperament requirements are currently being satisfied.
    /// Call this from CheckTemperament() implementations.
    /// </summary>
    /// <param name="satisfied">True if player is meeting temperament requirements.</param>
    /// <remarks>
    /// When satisfied:
    /// - Bonus affinity gain (+0.2/sec)
    /// - No punishment damage
    /// 
    /// When not satisfied:
    /// - No bonus affinity
    /// - Typically followed by punishment damage
    /// </remarks>
    protected void SetTemperamentSatisfied(bool satisfied)
    {
        isTemperamentSatisfied = satisfied;
    }
    
    #endregion
    
    #region Affinity Queries

    /// <summary>
    /// Gets a formatted string with current affinity information for debugging or UI display.
    /// </summary>
    /// <returns>Summary string with level, percentage, tether count, betrayals, and total time.</returns>
    public string GetAffinityInfo()
    {
        return AffinitySystem.Instance.GetAffinitySummary(entityId);
    }

    /// <summary>
    /// Gets the current affinity level with this entity.
    /// </summary>
    /// <returns>The current AffinityLevel enum value (Hostile through Ascended).</returns>
    public AffinityLevel GetAffinityLevel()
    {
        return AffinitySystem.Instance.GetAffinityLevel(entityId);
    }
    
    #endregion
    
    #region State Queries

    /// <summary>
    /// Checks if this entity is currently in a rampant (hostile) state.
    /// </summary>
    /// <returns>True if rampant state is active and entity may attack the player.</returns>
    public bool IsRampant()
    {
        return rampantState != null && rampantState.isRampant;
    }

    /// <summary>
    /// Gets the currently bound player, if any.
    /// </summary>
    /// <returns>The PlayerController of the tethered player, or null if not tethered.</returns>
    public PlayerController GetBoundPlayer()
    {
        return boundPlayer;
    }
    
    #endregion
}
