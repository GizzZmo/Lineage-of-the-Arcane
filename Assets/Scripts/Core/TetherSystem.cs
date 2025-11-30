using UnityEngine;

/// <summary>
/// Manages the "Risk/Reward" health drain logic for the Tether system.
/// The Tether is the magical bond between a player and a summoned entity (Parent, Scion, or Heir).
/// 
/// <para><b>Core Responsibilities:</b></para>
/// <list type="bullet">
///   <item><description>Initiating tether connections between players and entities</description></item>
///   <item><description>Continuous health drain while tethered (modified by affinity)</description></item>
///   <item><description>Temperament checking via the entity's CheckTemperament() method</description></item>
///   <item><description>Handling tether termination (clean sever vs. break)</description></item>
///   <item><description>Integration with AffinitySystem for relationship tracking</description></item>
/// </list>
/// 
/// <para><b>Tether Flow:</b></para>
/// <code>
/// InitiateTether() → Update Loop (MaintainTether + CheckTemperament) → SeverTether() or BreakTether()
/// </code>
/// 
/// <para><b>Termination Types:</b></para>
/// <list type="bullet">
///   <item><description>Clean Sever: Player manually severs with health above threshold → +5 affinity</description></item>
///   <item><description>Break: Health depleted or desperate sever → -15 affinity, triggers Rampant</description></item>
/// </list>
/// </summary>
/// <seealso cref="MagicParent"/>
/// <seealso cref="AffinitySystem"/>
/// <seealso cref="TetherVisualEffect"/>
public class TetherSystem : MonoBehaviour
{
    #region Configuration
    
    [Header("Tether Configuration")]
    /// <summary>The currently tethered magical entity. Null when no tether is active.</summary>
    public MagicParent activeSummon;
    
    /// <summary>Reference to the player controller for health management and combat tracking.</summary>
    public PlayerController player;
    
    /// <summary>Whether a tether connection is currently active.</summary>
    public bool isTethered = false;
    
    [Header("Tether Visual Settings")]
    /// <summary>Color used for tether visualization (passed to TetherVisualEffect).</summary>
    public Color tetherColor = Color.blue;
    
    /// <summary>Width of the tether line renderer.</summary>
    public float tetherWidth = 0.1f;
    
    [Header("Affinity Integration")]
    /// <summary>
    /// Minimum health percentage required for a clean tether sever.
    /// Below this threshold, severing counts as a betrayal.
    /// Default: 0.2 (20% health)
    /// </summary>
    [Tooltip("Minimum health percentage to safely sever tether")]
    public float safeSeverThreshold = 0.2f;
    
    #endregion
    
    #region Runtime State
    
    [Header("Runtime Info")]
    /// <summary>Current tether cost per second after affinity modifiers are applied.</summary>
    [SerializeField] private float currentTetherCost;
    
    /// <summary>Duration of the current tether session in seconds.</summary>
    [SerializeField] private float sessionDuration;
    
    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Main update loop handling tether maintenance while active.
    /// Drains player health, checks temperament, and tracks session duration.
    /// </summary>
    void Update()
    {
        if (isTethered && activeSummon != null)
        {
            MaintainTether();                    // Drain health, build affinity
            activeSummon.CheckTemperament();     // Validate player behavior
            sessionDuration += Time.deltaTime;   // Track session length
        }
    }
    
    #endregion
    
    #region Tether Initiation

    /// <summary>
    /// Initiates a tether connection between the player and a magical entity.
    /// Triggers the entity's summon behavior, environmental effects, and affinity tracking.
    /// </summary>
    /// <param name="summon">The MagicParent entity to tether with.</param>
    /// <example>
    /// <code>
    /// // Basic tether initiation
    /// MagicParent ignis = FindObjectOfType&lt;IgnisMater&gt;();
    /// tetherSystem.InitiateTether(ignis);
    /// </code>
    /// </example>
    public void InitiateTether(MagicParent summon)
    {
        // Validate references before initiating
        if (player == null)
        {
            Debug.LogError("Cannot initiate tether: Player is null!");
            return;
        }
        
        if (summon == null)
        {
            Debug.LogError("Cannot initiate tether: Summon is null!");
            return;
        }
        
        // Set up the tether connection
        activeSummon = summon;
        isTethered = true;
        sessionDuration = 0f;
        
        // Trigger the entity's summon behavior (environmental shift, affinity check, etc.)
        activeSummon.OnSummon(player);
        
        // Cache the affinity-modified tether cost for the update loop
        currentTetherCost = activeSummon.GetModifiedTetherCost();
        
        Debug.Log($"Tether initiated with {activeSummon.entityName}");
        Debug.Log($"[TETHER] Modified cost per second: {currentTetherCost:F2}");
    }
    
    #endregion
    
    #region Tether Maintenance

    /// <summary>
    /// Called every frame while tethered to drain player health and accumulate affinity.
    /// If player health is insufficient, triggers BreakTether().
    /// </summary>
    /// <remarks>
    /// Cost calculation: currentTetherCost * Time.deltaTime
    /// The cost is already modified by affinity level (retrieved from the entity).
    /// </remarks>
    void MaintainTether()
    {
        // Refresh the affinity-modified tether cost (may change during tether)
        currentTetherCost = activeSummon.GetModifiedTetherCost();
        float cost = currentTetherCost * Time.deltaTime;
        
        if (player.currentHealth > cost)
        {
            // Drain health - the price of power
            player.currentHealth -= cost;
            
            // Build affinity through successful tethering
            activeSummon.OnTetherMaintained(Time.deltaTime);
        }
        else
        {
            // Health depleted - tether breaks violently
            BreakTether();
        }
    }
    
    #endregion
    
    #region Tether Termination

    /// <summary>
    /// Forcefully breaks the tether connection (health depleted or desperate sever).
    /// Triggers betrayal penalty in affinity system and rampant state in entity.
    /// </summary>
    /// <remarks>
    /// Consequences:
    /// - Records betrayal in AffinitySystem (-15 affinity default)
    /// - Entity enters Rampant state (hostile AI)
    /// - After 3 betrayals with low affinity, entity becomes Hostile
    /// </remarks>
    void BreakTether()
    {
        isTethered = false;
        Debug.LogWarning("THE TETHER SNAPS! The Parent goes RAMPANT.");
        Debug.Log($"[TETHER] Session ended after {sessionDuration:F1} seconds");
        
        // Trigger the entity's break behavior (rampant state, betrayal recording)
        if (activeSummon != null)
        {
            activeSummon.OnTetherBroken();
        }
        
        sessionDuration = 0f;
    }
    
    /// <summary>
    /// Manually severs the tether connection.
    /// Automatically determines whether this is a clean sever or forced break based on health.
    /// </summary>
    /// <remarks>
    /// If health percentage >= safeSeverThreshold (default 20%):
    ///   → Clean sever, grants +5 affinity bonus
    /// If health percentage < safeSeverThreshold:
    ///   → Treated as a break, triggers betrayal
    /// </remarks>
    public void SeverTether()
    {
        if (!isTethered) return;
        
        float healthPercentage = player.currentHealth / player.maxHealth;
        
        if (healthPercentage >= safeSeverThreshold)
        {
            // Clean sever - player had enough health remaining
            SeverTetherCleanly();
        }
        else
        {
            // Desperate sever at low health - still counts as betrayal
            BreakTether();
        }
    }
    
    /// <summary>
    /// Severs the tether cleanly without triggering rampant state.
    /// Grants affinity bonus (+5 default) for successful tethering.
    /// </summary>
    /// <remarks>
    /// Call this directly to force a clean sever regardless of health.
    /// Normally, use SeverTether() which auto-detects based on health.
    /// </remarks>
    public void SeverTetherCleanly()
    {
        if (!isTethered) return;
        
        isTethered = false;
        Debug.Log($"[TETHER] Tether severed cleanly after {sessionDuration:F1} seconds");
        
        // Trigger the entity's clean termination (affinity bonus, no rampant)
        if (activeSummon != null)
        {
            activeSummon.OnTetherSeveredCleanly();
        }
        
        sessionDuration = 0f;
    }
    
    #endregion
    
    #region Queries
    
    /// <summary>
    /// Gets formatted information about the current tether session for UI or debugging.
    /// </summary>
    /// <returns>Multi-line string with entity name, duration, cost, and affinity level.</returns>
    public string GetTetherSessionInfo()
    {
        if (!isTethered || activeSummon == null)
        {
            return "No active tether";
        }
        
        return $"Tethered to: {activeSummon.entityName}\n" +
               $"Duration: {sessionDuration:F1}s\n" +
               $"Cost/sec: {currentTetherCost:F2}\n" +
               $"Affinity: {activeSummon.GetAffinityLevel()}";
    }
    
    /// <summary>
    /// Gets the current tether cost per second after affinity modifiers.
    /// </summary>
    /// <returns>The current health drain rate per second.</returns>
    public float GetCurrentTetherCost()
    {
        return currentTetherCost;
    }
    
    /// <summary>
    /// Gets how long the current tether session has been active.
    /// </summary>
    /// <returns>Session duration in seconds, or 0 if not tethered.</returns>
    public float GetSessionDuration()
    {
        return sessionDuration;
    }
    
    #endregion
}
