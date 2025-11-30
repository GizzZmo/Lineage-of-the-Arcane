using UnityEngine;

/// <summary>
/// The Blueprint for all "Parents of Magic".
/// This abstract class defines what a "Parent" is and handles personality traits.
/// RequireComponent ensures the RampantState is always present on the GameObject.
/// </summary>
[RequireComponent(typeof(RampantState))]
public abstract class MagicParent : MonoBehaviour
{
    [Header("Entity Stats")]
    public string entityName;
    public float tetherCostPerSecond = 5.0f; // How much Health/Sanity it costs
    [Range(0, 100)] public float defianceLevel = 0f; // Chance to ignore orders

    [Header("Rampant Configuration")]
    public RampantBehavior rampantBehavior = RampantBehavior.Aggressive;
    public float rampantDuration = 30f;
    public float rampantDamage = 15f;

    [Header("Affinity System")]
    [Tooltip("Unique identifier for affinity tracking")]
    public string entityId;
    [Tooltip("Whether temperament requirements are currently being satisfied")]
    protected bool isTemperamentSatisfied = true;
    [Tooltip("Current tether cost after affinity modifiers")]
    protected float modifiedTetherCost;
    [Tooltip("Whether the special ability is available")]
    protected bool hasSpecialAbility = false;

    protected PlayerController boundPlayer;
    protected RampantState rampantState;

    protected virtual void Awake()
    {
        // Get the RampantState component (guaranteed to exist via RequireComponent)
        rampantState = GetComponent<RampantState>();
        
        // Configure rampant state based on this parent's settings
        ConfigureRampantState();
        
        // Initialize entity ID if not set
        if (string.IsNullOrEmpty(entityId))
        {
            entityId = GetType().Name;
        }
    }

    /// <summary>
    /// Configures the RampantState component with this Parent's settings.
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

    /// <summary>
    /// Called when the player initiates the Tether.
    /// </summary>
    /// <param name="player">The player forming the tether.</param>
    public virtual void OnSummon(PlayerController player)
    {
        boundPlayer = player;
        
        // Apply affinity-based cost modifier
        UpdateTetherCostFromAffinity();
        
        // Check if special ability is unlocked
        hasSpecialAbility = AffinitySystem.Instance.IsAbilityUnlocked(entityId);
        
        Debug.Log($"{entityName} has entered the reality. The Tether is formed.");
        Debug.Log($"[AFFINITY] Current level: {AffinitySystem.Instance.GetAffinityLevel(entityId)}, " +
                  $"Cost modifier: {AffinitySystem.Instance.GetTetherCostMultiplier(entityId):F2}x");
        
        ApplyEnvironmentalShift();
        
        if (hasSpecialAbility)
        {
            OnSpecialAbilityAvailable();
        }
    }

    /// <summary>
    /// Updates the tether cost based on current affinity level.
    /// </summary>
    protected void UpdateTetherCostFromAffinity()
    {
        float costMultiplier = AffinitySystem.Instance.GetTetherCostMultiplier(entityId);
        modifiedTetherCost = tetherCostPerSecond * costMultiplier;
    }

    /// <summary>
    /// Gets the current tether cost per second after affinity modifiers.
    /// </summary>
    /// <returns>The modified tether cost.</returns>
    public float GetModifiedTetherCost()
    {
        return modifiedTetherCost > 0 ? modifiedTetherCost : tetherCostPerSecond;
    }

    /// <summary>
    /// Called during tether maintenance to update affinity.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public virtual void OnTetherMaintained(float deltaTime)
    {
        AffinitySystem.Instance.AddTetherAffinity(entityId, deltaTime, isTemperamentSatisfied);
    }

    /// <summary>
    /// Called when the special ability becomes available through affinity.
    /// Override to implement entity-specific special abilities.
    /// </summary>
    protected virtual void OnSpecialAbilityAvailable()
    {
        Debug.Log($"{entityName}'s special ability is now available!");
    }

    /// <summary>
    /// Activates the special ability if unlocked.
    /// Override to implement entity-specific abilities.
    /// </summary>
    public virtual void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning($"Special ability for {entityName} is not unlocked yet!");
            return;
        }
        
        Debug.Log($"{entityName} activates special ability!");
    }

    /// <summary>
    /// Every Parent changes the game world physics/lighting.
    /// </summary>
    protected abstract void ApplyEnvironmentalShift();

    /// <summary>
    /// The unique passive rule (e.g., "Must keep attacking").
    /// Each Parent has different temperament requirements.
    /// </summary>
    public abstract void CheckTemperament();
    
    /// <summary>
    /// Called when the tether breaks.
    /// Triggers the Rampant state behavior.
    /// </summary>
    public virtual void OnTetherBroken()
    {
        Debug.LogWarning($"{entityName} is now RAMPANT! The bond has been severed.");
        
        // Record the betrayal in the affinity system
        AffinitySystem.Instance.OnTetherBetrayal(entityId);
        
        // Trigger rampant state
        if (rampantState != null && boundPlayer != null)
        {
            rampantState.EnterRampantState(boundPlayer.transform);
        }
        
        // Clear the bound player reference
        boundPlayer = null;
    }

    /// <summary>
    /// Called when the tether is manually severed cleanly.
    /// This does not trigger rampant state and grants affinity.
    /// </summary>
    public virtual void OnTetherSeveredCleanly()
    {
        Debug.Log($"{entityName} returns to the void peacefully.");
        
        // Record successful tether in the affinity system
        AffinitySystem.Instance.OnSuccessfulTether(entityId);
        
        // Clear the bound player reference
        boundPlayer = null;
    }

    /// <summary>
    /// Sets whether temperament requirements are being satisfied.
    /// </summary>
    /// <param name="satisfied">True if temperament is satisfied.</param>
    protected void SetTemperamentSatisfied(bool satisfied)
    {
        isTemperamentSatisfied = satisfied;
    }

    /// <summary>
    /// Gets current affinity information for this entity.
    /// </summary>
    /// <returns>A summary string of affinity data.</returns>
    public string GetAffinityInfo()
    {
        return AffinitySystem.Instance.GetAffinitySummary(entityId);
    }

    /// <summary>
    /// Gets the current affinity level for this entity.
    /// </summary>
    /// <returns>The affinity level.</returns>
    public AffinityLevel GetAffinityLevel()
    {
        return AffinitySystem.Instance.GetAffinityLevel(entityId);
    }

    /// <summary>
    /// Checks if this Parent is currently in a rampant state.
    /// </summary>
    public bool IsRampant()
    {
        return rampantState != null && rampantState.isRampant;
    }

    /// <summary>
    /// Gets the currently bound player.
    /// </summary>
    public PlayerController GetBoundPlayer()
    {
        return boundPlayer;
    }
}
