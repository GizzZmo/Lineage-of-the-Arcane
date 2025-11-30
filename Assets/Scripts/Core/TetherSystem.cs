using UnityEngine;

/// <summary>
/// Handles the "Risk/Reward" health drain logic for the Tether system.
/// This system connects players to their summoned Parents.
/// Integrates with the AffinitySystem to track relationship progress.
/// </summary>
public class TetherSystem : MonoBehaviour
{
    [Header("Tether Configuration")]
    public MagicParent activeSummon;
    public PlayerController player;
    public bool isTethered = false;
    
    [Header("Tether Visual Settings")]
    public Color tetherColor = Color.blue;
    public float tetherWidth = 0.1f;
    
    [Header("Affinity Integration")]
    [Tooltip("Minimum health percentage to safely sever tether")]
    public float safeSeverThreshold = 0.2f;
    
    [Header("Runtime Info")]
    [SerializeField] private float currentTetherCost;
    [SerializeField] private float sessionDuration;

    void Update()
    {
        if (isTethered && activeSummon != null)
        {
            MaintainTether();
            activeSummon.CheckTemperament(); // Check if the Parent is angry
            sessionDuration += Time.deltaTime;
        }
    }

    /// <summary>
    /// Initiates a tether between the player and a summon.
    /// </summary>
    /// <param name="summon">The MagicParent to tether.</param>
    public void InitiateTether(MagicParent summon)
    {
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
        
        activeSummon = summon;
        isTethered = true;
        sessionDuration = 0f;
        activeSummon.OnSummon(player);
        
        // Get the affinity-modified tether cost
        currentTetherCost = activeSummon.GetModifiedTetherCost();
        
        Debug.Log($"Tether initiated with {activeSummon.entityName}");
        Debug.Log($"[TETHER] Modified cost per second: {currentTetherCost:F2}");
    }

    void MaintainTether()
    {
        // Use the affinity-modified tether cost (single call to avoid redundant calculations)
        currentTetherCost = activeSummon.GetModifiedTetherCost();
        float cost = currentTetherCost * Time.deltaTime;
        
        if (player.currentHealth > cost)
        {
            player.currentHealth -= cost;
            
            // Update affinity for time spent tethered
            activeSummon.OnTetherMaintained(Time.deltaTime);
        }
        else
        {
            BreakTether();
        }
    }

    void BreakTether()
    {
        isTethered = false;
        Debug.LogWarning("THE TETHER SNAPS! The Parent goes RAMPANT.");
        Debug.Log($"[TETHER] Session ended after {sessionDuration:F1} seconds");
        
        if (activeSummon != null)
        {
            activeSummon.OnTetherBroken();
        }
        
        sessionDuration = 0f;
    }
    
    /// <summary>
    /// Manually severs the tether connection.
    /// If severed safely (player has enough health), grants affinity bonus.
    /// If severed while health is critical, triggers a betrayal.
    /// </summary>
    public void SeverTether()
    {
        if (!isTethered) return;
        
        float healthPercentage = player.currentHealth / player.maxHealth;
        
        if (healthPercentage >= safeSeverThreshold)
        {
            // Clean sever - grants affinity
            SeverTetherCleanly();
        }
        else
        {
            // Desperate sever - still counts as a break
            BreakTether();
        }
    }
    
    /// <summary>
    /// Severs the tether cleanly without triggering rampant state.
    /// Grants affinity bonus.
    /// </summary>
    public void SeverTetherCleanly()
    {
        if (!isTethered) return;
        
        isTethered = false;
        Debug.Log($"[TETHER] Tether severed cleanly after {sessionDuration:F1} seconds");
        
        if (activeSummon != null)
        {
            activeSummon.OnTetherSeveredCleanly();
        }
        
        sessionDuration = 0f;
    }
    
    /// <summary>
    /// Gets information about the current tether session.
    /// </summary>
    /// <returns>A string with session information.</returns>
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
    /// Gets the current tether cost per second (after affinity modifiers).
    /// </summary>
    /// <returns>The current tether cost.</returns>
    public float GetCurrentTetherCost()
    {
        return currentTetherCost;
    }
    
    /// <summary>
    /// Gets the session duration in seconds.
    /// </summary>
    /// <returns>The session duration.</returns>
    public float GetSessionDuration()
    {
        return sessionDuration;
    }
}
