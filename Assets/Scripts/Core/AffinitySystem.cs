using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the different affinity levels a player can achieve with an entity.
/// </summary>
public enum AffinityLevel
{
    Hostile = -1,      // Entity remembers betrayal, increased tether cost
    Stranger = 0,      // No relationship, default tether cost
    Acquainted = 1,    // Minor affinity, slightly reduced cost
    Bonded = 2,        // Moderate affinity, noticeable benefits
    Devoted = 3,       // High affinity, significant benefits
    Ascended = 4       // Maximum affinity, unlocks special abilities
}

/// <summary>
/// Represents the affinity data between a player and a specific entity type.
/// </summary>
[System.Serializable]
public class AffinityData
{
    public string entityId;           // Unique identifier for the entity type
    public float currentAffinity;     // Current affinity points (0-100)
    public AffinityLevel level;       // Current affinity level
    public int successfulTethers;     // Number of successful tethering sessions
    public int betrayals;             // Number of times tether was broken unexpectedly
    public float totalTimeTethered;   // Total time spent tethered to this entity
    public bool hasUnlockedAbility;   // Whether special ability is unlocked
    
    // Affinity thresholds for each level
    public const float ACQUAINTED_THRESHOLD = 20f;
    public const float BONDED_THRESHOLD = 40f;
    public const float DEVOTED_THRESHOLD = 70f;
    public const float ASCENDED_THRESHOLD = 100f;
    
    public AffinityData(string entityId)
    {
        this.entityId = entityId;
        this.currentAffinity = 0f;
        this.level = AffinityLevel.Stranger;
        this.successfulTethers = 0;
        this.betrayals = 0;
        this.totalTimeTethered = 0f;
        this.hasUnlockedAbility = false;
    }
    
    /// <summary>
    /// Updates the affinity level based on current affinity points.
    /// </summary>
    public void UpdateLevel()
    {
        // Check for hostile status (too many betrayals)
        if (betrayals >= 3 && currentAffinity < ACQUAINTED_THRESHOLD)
        {
            level = AffinityLevel.Hostile;
            return;
        }
        
        // Determine level based on affinity points
        if (currentAffinity >= ASCENDED_THRESHOLD)
        {
            level = AffinityLevel.Ascended;
            hasUnlockedAbility = true;
        }
        else if (currentAffinity >= DEVOTED_THRESHOLD)
        {
            level = AffinityLevel.Devoted;
        }
        else if (currentAffinity >= BONDED_THRESHOLD)
        {
            level = AffinityLevel.Bonded;
        }
        else if (currentAffinity >= ACQUAINTED_THRESHOLD)
        {
            level = AffinityLevel.Acquainted;
        }
        else
        {
            level = AffinityLevel.Stranger;
        }
    }
}

/// <summary>
/// Manages the Evolution/Affinity system for player-entity relationships.
/// This system tracks how well players work with different magical entities
/// and provides benefits or penalties based on their relationship history.
/// </summary>
public class AffinitySystem : MonoBehaviour
{
    [Header("Affinity Configuration")]
    [Tooltip("Base affinity gained per successful second of tethering")]
    public float affinityGainRate = 0.5f;
    
    [Tooltip("Affinity lost when tether breaks unexpectedly")]
    public float betrayalPenalty = 15f;
    
    [Tooltip("Bonus affinity gained when temperament requirements are met")]
    public float temperamentBonus = 0.2f;
    
    [Tooltip("Affinity gained for a cleanly severed tether")]
    public float cleanSeverBonus = 5f;
    
    [Header("Tether Cost Modifiers")]
    [Tooltip("Cost multiplier for Hostile level")]
    public float hostileCostMultiplier = 1.5f;
    
    [Tooltip("Cost multiplier for Acquainted level")]
    public float acquaintedCostMultiplier = 0.9f;
    
    [Tooltip("Cost multiplier for Bonded level")]
    public float bondedCostMultiplier = 0.8f;
    
    [Tooltip("Cost multiplier for Devoted level")]
    public float devotedCostMultiplier = 0.65f;
    
    [Tooltip("Cost multiplier for Ascended level")]
    public float ascendedCostMultiplier = 0.5f;
    
    [Header("Runtime State")]
    [SerializeField] private Dictionary<string, AffinityData> affinityRecords;
    
    // Singleton instance
    private static AffinitySystem instance;
    public static AffinitySystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AffinitySystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AffinitySystem");
                    instance = go.AddComponent<AffinitySystem>();
                }
            }
            return instance;
        }
    }
    
    // Events for UI updates and game reactions
    public System.Action<string, AffinityLevel> OnAffinityLevelChanged;
    public System.Action<string, float> OnAffinityGained;
    public System.Action<string, float> OnAffinityLost;
    public System.Action<string> OnAbilityUnlocked;
    
    void Awake()
    {
        // Implement singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        affinityRecords = new Dictionary<string, AffinityData>();
    }
    
    /// <summary>
    /// Gets or creates affinity data for a specific entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity type.</param>
    /// <returns>The affinity data for the entity.</returns>
    public AffinityData GetAffinityData(string entityId)
    {
        if (!affinityRecords.ContainsKey(entityId))
        {
            affinityRecords[entityId] = new AffinityData(entityId);
        }
        return affinityRecords[entityId];
    }
    
    /// <summary>
    /// Gets the current affinity level for an entity.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>The current affinity level.</returns>
    public AffinityLevel GetAffinityLevel(string entityId)
    {
        return GetAffinityData(entityId).level;
    }
    
    /// <summary>
    /// Gets the tether cost multiplier based on affinity level.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>The cost multiplier to apply to tether drain.</returns>
    public float GetTetherCostMultiplier(string entityId)
    {
        AffinityLevel level = GetAffinityLevel(entityId);
        
        switch (level)
        {
            case AffinityLevel.Hostile:
                return hostileCostMultiplier;
            case AffinityLevel.Acquainted:
                return acquaintedCostMultiplier;
            case AffinityLevel.Bonded:
                return bondedCostMultiplier;
            case AffinityLevel.Devoted:
                return devotedCostMultiplier;
            case AffinityLevel.Ascended:
                return ascendedCostMultiplier;
            default:
                return 1f; // Stranger level
        }
    }
    
    /// <summary>
    /// Adds affinity points for continuous tethering.
    /// Called during the tether update loop.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    /// <param name="temperamentSatisfied">Whether the player is meeting temperament requirements.</param>
    public void AddTetherAffinity(string entityId, float deltaTime, bool temperamentSatisfied)
    {
        AffinityData data = GetAffinityData(entityId);
        AffinityLevel previousLevel = data.level;
        
        float gain = affinityGainRate * deltaTime;
        
        // Bonus for meeting temperament requirements
        if (temperamentSatisfied)
        {
            gain += temperamentBonus * deltaTime;
        }
        
        data.currentAffinity = Mathf.Min(data.currentAffinity + gain, 100f);
        data.totalTimeTethered += deltaTime;
        data.UpdateLevel();
        
        // Trigger events
        OnAffinityGained?.Invoke(entityId, gain);
        
        if (data.level != previousLevel)
        {
            OnAffinityLevelChanged?.Invoke(entityId, data.level);
            Debug.Log($"[AFFINITY] {entityId} affinity level changed to {data.level}!");
            
            if (data.level == AffinityLevel.Ascended && data.hasUnlockedAbility)
            {
                OnAbilityUnlocked?.Invoke(entityId);
                Debug.Log($"[AFFINITY] {entityId} special ability UNLOCKED!");
            }
        }
    }
    
    /// <summary>
    /// Called when a tether is successfully completed (manually severed or session ended).
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    public void OnSuccessfulTether(string entityId)
    {
        AffinityData data = GetAffinityData(entityId);
        data.successfulTethers++;
        
        // Add clean sever bonus
        float bonus = cleanSeverBonus;
        
        // Extra bonus for maintaining temperament
        data.currentAffinity = Mathf.Min(data.currentAffinity + bonus, 100f);
        data.UpdateLevel();
        
        Debug.Log($"[AFFINITY] Successful tether with {entityId}. Total: {data.successfulTethers}");
        OnAffinityGained?.Invoke(entityId, bonus);
    }
    
    /// <summary>
    /// Called when a tether is successfully completed with a bonus amount.
    /// Used by entities that grant extra affinity (like Heirs).
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="bonusAmount">Additional affinity bonus to add.</param>
    public void OnSuccessfulTetherWithBonus(string entityId, float bonusAmount)
    {
        AffinityData data = GetAffinityData(entityId);
        AffinityLevel previousLevel = data.level;
        data.successfulTethers++;
        
        // Add clean sever bonus plus the entity-specific bonus
        float totalBonus = cleanSeverBonus + bonusAmount;
        
        data.currentAffinity = Mathf.Min(data.currentAffinity + totalBonus, 100f);
        data.UpdateLevel();
        
        Debug.Log($"[AFFINITY] Successful tether with {entityId} (bonus: {bonusAmount:F1}). Total: {data.successfulTethers}");
        OnAffinityGained?.Invoke(entityId, totalBonus);
        
        if (data.level != previousLevel)
        {
            OnAffinityLevelChanged?.Invoke(entityId, data.level);
            Debug.Log($"[AFFINITY] {entityId} affinity level changed to {data.level}!");
        }
    }
    
    /// <summary>
    /// Called when a tether is broken unexpectedly (betrayal).
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    public void OnTetherBetrayal(string entityId)
    {
        OnTetherBetrayalWithPenalty(entityId, betrayalPenalty);
    }
    
    /// <summary>
    /// Called when a tether is broken unexpectedly with a custom penalty.
    /// Used by entities with reduced betrayal penalties (like Heirs).
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="penalty">The amount of affinity to lose.</param>
    public void OnTetherBetrayalWithPenalty(string entityId, float penalty)
    {
        AffinityData data = GetAffinityData(entityId);
        AffinityLevel previousLevel = data.level;
        
        data.betrayals++;
        data.currentAffinity = Mathf.Max(data.currentAffinity - penalty, 0f);
        data.UpdateLevel();
        
        Debug.LogWarning($"[AFFINITY] Betrayal with {entityId}! Lost {penalty:F1} affinity. Total betrayals: {data.betrayals}");
        OnAffinityLost?.Invoke(entityId, penalty);
        
        if (data.level != previousLevel)
        {
            OnAffinityLevelChanged?.Invoke(entityId, data.level);
            Debug.LogWarning($"[AFFINITY] {entityId} affinity level dropped to {data.level}!");
        }
    }
    
    /// <summary>
    /// Checks if a special ability is unlocked for an entity.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>True if the ability is unlocked.</returns>
    public bool IsAbilityUnlocked(string entityId)
    {
        return GetAffinityData(entityId).hasUnlockedAbility;
    }
    
    /// <summary>
    /// Gets the current affinity percentage for an entity (0-100).
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>The affinity percentage.</returns>
    public float GetAffinityPercentage(string entityId)
    {
        return GetAffinityData(entityId).currentAffinity;
    }
    
    /// <summary>
    /// Gets the progress to the next affinity level.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>Progress percentage to next level (0-1).</returns>
    public float GetProgressToNextLevel(string entityId)
    {
        AffinityData data = GetAffinityData(entityId);
        
        float currentThreshold = 0f;
        float nextThreshold = AffinityData.ACQUAINTED_THRESHOLD;
        
        switch (data.level)
        {
            case AffinityLevel.Hostile:
            case AffinityLevel.Stranger:
                currentThreshold = 0f;
                nextThreshold = AffinityData.ACQUAINTED_THRESHOLD;
                break;
            case AffinityLevel.Acquainted:
                currentThreshold = AffinityData.ACQUAINTED_THRESHOLD;
                nextThreshold = AffinityData.BONDED_THRESHOLD;
                break;
            case AffinityLevel.Bonded:
                currentThreshold = AffinityData.BONDED_THRESHOLD;
                nextThreshold = AffinityData.DEVOTED_THRESHOLD;
                break;
            case AffinityLevel.Devoted:
                currentThreshold = AffinityData.DEVOTED_THRESHOLD;
                nextThreshold = AffinityData.ASCENDED_THRESHOLD;
                break;
            case AffinityLevel.Ascended:
                return 1f; // Max level reached
        }
        
        float range = nextThreshold - currentThreshold;
        float progress = data.currentAffinity - currentThreshold;
        
        return Mathf.Clamp01(progress / range);
    }
    
    /// <summary>
    /// Resets affinity data for a specific entity. Use with caution.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    public void ResetAffinity(string entityId)
    {
        if (affinityRecords.ContainsKey(entityId))
        {
            affinityRecords[entityId] = new AffinityData(entityId);
            Debug.Log($"[AFFINITY] Reset affinity data for {entityId}");
        }
    }
    
    /// <summary>
    /// Gets a summary string for debugging or UI display.
    /// </summary>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>A formatted string with affinity information.</returns>
    public string GetAffinitySummary(string entityId)
    {
        AffinityData data = GetAffinityData(entityId);
        return $"{entityId}: Level {data.level} ({data.currentAffinity:F1}%) | " +
               $"Tethers: {data.successfulTethers} | Betrayals: {data.betrayals} | " +
               $"Time: {data.totalTimeTethered:F1}s";
    }
}
