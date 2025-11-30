using UnityEngine;

/// <summary>
/// Wound Scion - A Tier 1 descendant of Dolor Mater.
/// Less demanding than Dolor Mater but still appreciates sacrifice.
/// Tethering with Wound also builds affinity with Dolor Mater.
/// </summary>
public class WoundScion : Scion
{
    [Header("Wound Settings")]
    public float sacrificeInterval = 8.0f; // More lenient than Dolor Mater
    public float minimumSacrifice = 3.0f; // Lower sacrifice threshold
    public float basePunishmentDamage = 3.0f;
    public Color localAmbientColor = new Color(0.6f, 0.2f, 0.3f); // Dusky red
    
    private float lastSacrificeTime;
    private float damageAccumulated;
    private float lastTemperamentCheckTime;
    
    private void Start()
    {
        entityName = "Wound, The Scar";
        entityId = "WoundScion";
        parentLineage = "Dolor Mater";
        tetherCostPerSecond = 4.5f; // Moderate cost
        
        // Configure less severe rampant behavior
        rampantBehavior = RampantBehavior.Vengeful;
        rampantDuration = 18f;
        rampantDamage = 14f;
        
        ConfigureRampantState();
    }
    
    protected override void ApplyLocalEnvironmentalShift()
    {
        // Apply a localized pain effect
        Debug.Log("A dull ache permeates the area around Wound. Pain becomes palpable.");
        
        // Subtle ambient light change
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, localAmbientColor, 0.3f);
    }
    
    public override void OnSummon(PlayerController player)
    {
        base.OnSummon(player);
        
        // Initialize sacrifice tracking
        lastSacrificeTime = Time.time;
        damageAccumulated = 0f;
    }
    
    /// <summary>
    /// Called when the player takes damage while tethered.
    /// This should be hooked into the damage system or called by PlayerController.
    /// </summary>
    /// <param name="damage">Amount of damage taken. Must be non-negative.</param>
    public void OnPlayerTakeDamage(float damage)
    {
        // Validate input
        if (damage < 0)
        {
            Debug.LogWarning("[WOUND] OnPlayerTakeDamage received negative damage value.");
            return;
        }
        
        damageAccumulated += damage;
        Debug.Log($"Wound appreciates your pain. ({damageAccumulated:F1}/{minimumSacrifice} this cycle)");
    }
    
    protected override void PerformTemperamentCheck()
    {
        if (boundPlayer == null) return;
        
        // Only check periodically
        if (Time.time - lastTemperamentCheckTime < temperamentTolerance)
        {
            return;
        }
        
        lastTemperamentCheckTime = Time.time;
        
        // Check sacrifice requirements at intervals
        if (Time.time - lastSacrificeTime > sacrificeInterval)
        {
            if (damageAccumulated < minimumSacrifice)
            {
                ApplyPunishment(basePunishmentDamage);
                Debug.Log("Wound yearns for your suffering.");
            }
            else
            {
                ApproveTemperament();
                Debug.Log($"Wound accepts your offering of {damageAccumulated:F1} pain.");
            }
            
            // Reset for next interval
            damageAccumulated = 0f;
            lastSacrificeTime = Time.time;
        }
        else
        {
            // During interval, approve temperament
            ApproveTemperament();
        }
    }
    
    public override void OnTetherBroken()
    {
        base.OnTetherBroken();
        Debug.Log("Wound festers and throbs with anger!");
        
        // Reset ambient light effect
        RenderSettings.ambientLight = Color.white * 0.5f;
    }
    
    /// <summary>
    /// Gets the parent entity ID for this scion.
    /// </summary>
    protected override string GetParentEntityId()
    {
        // Override to map to new parent
        if (parentLineage == "Dolor Mater" || parentLineage == "Dolor")
        {
            return "DolorMater";
        }
        return base.GetParentEntityId();
    }
    
    /// <summary>
    /// Gets the current damage accumulated this sacrifice cycle.
    /// </summary>
    public float GetDamageAccumulated()
    {
        return damageAccumulated;
    }
}
