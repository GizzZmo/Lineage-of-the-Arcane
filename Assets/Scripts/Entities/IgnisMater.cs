using UnityEngine;

/// <summary>
/// Ignis Mater, The Combustion - The Fire Mother entity.
/// She demands aggression and punishes hesitation.
/// 
/// Special Ability (Ascended): "Inferno Embrace" - Temporary invulnerability while dealing AoE damage.
/// </summary>
public class IgnisMater : MagicParent
{
    [Header("Ignis Settings")]
    public float hesitationThreshold = 3.0f; // Seconds without attacking before punishment
    public float punishmentDamage = 5.0f;
    public Color ambientColor = Color.red;
    
    [Header("Special Ability - Inferno Embrace")]
    [Tooltip("Duration of the special ability in seconds")]
    public float infernoAbilityDuration = 5f;
    [Tooltip("Damage dealt per second during ability")]
    public float infernoDamagePerSecond = 10f;
    [Tooltip("Radius of the inferno damage area")]
    public float infernoRadius = 8f;
    [Tooltip("Cooldown between ability uses")]
    public float abilityCooldown = 30f;
    
    private float lastAbilityUseTime = -999f;
    private bool isInfernoActive = false;
    private float infernoStartTime;
    
    private void Start()
    {
        entityName = "Ignis Mater, The Combustion";
        entityId = "IgnisMater";
        tetherCostPerSecond = 10.0f; // High cost
        
        // Configure rampant behavior specific to Ignis
        rampantBehavior = RampantBehavior.Aggressive;
        rampantDuration = 45f; // Longer rampant duration
        rampantDamage = 20f;   // High damage when rampant
        
        ConfigureRampantState();
    }
    
    void Update()
    {
        // Update inferno ability if active
        if (isInfernoActive)
        {
            UpdateInfernoAbility();
        }
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to turn the floor to lava or make lighting red
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("The world heats up. Ignis is watching.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: If player hasn't attacked in threshold seconds, punish them
        if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
        {
            SetTemperamentSatisfied(false);
            PunishPlayer();
        }
        else
        {
            SetTemperamentSatisfied(true);
        }
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Ignis burns you for your hesitation!");
        }
    }
    
    public override void OnTetherBroken()
    {
        base.OnTetherBroken();
        isInfernoActive = false;
        Debug.LogWarning("Ignis Mater erupts in fury! Everything burns!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = Color.red * 2f;
    }
    
    /// <summary>
    /// Called when the special ability becomes available.
    /// </summary>
    protected override void OnSpecialAbilityAvailable()
    {
        Debug.Log("[IGNIS] Inferno Embrace ability is now available! Press the ability key to activate.");
    }
    
    /// <summary>
    /// Activates the Inferno Embrace special ability.
    /// </summary>
    public override void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning("Inferno Embrace is not unlocked. Reach Ascended affinity with Ignis Mater.");
            return;
        }
        
        if (Time.time - lastAbilityUseTime < abilityCooldown)
        {
            float remaining = abilityCooldown - (Time.time - lastAbilityUseTime);
            Debug.Log($"[IGNIS] Inferno Embrace on cooldown. {remaining:F1}s remaining.");
            return;
        }
        
        if (boundPlayer == null)
        {
            Debug.LogWarning("Cannot activate ability without a tethered player.");
            return;
        }
        
        // Activate the inferno
        isInfernoActive = true;
        infernoStartTime = Time.time;
        lastAbilityUseTime = Time.time;
        
        Debug.Log("[IGNIS] INFERNO EMBRACE ACTIVATED! You are wreathed in flame!");
        
        // Visual effect: Intensify ambient
        RenderSettings.ambientLight = Color.red * 3f;
    }
    
    /// <summary>
    /// Updates the Inferno Embrace ability while active.
    /// </summary>
    private void UpdateInfernoAbility()
    {
        if (Time.time - infernoStartTime >= infernoAbilityDuration)
        {
            EndInfernoAbility();
            return;
        }
        
        // Deal AoE damage to enemies (placeholder - would hit enemies in actual implementation)
        // In a full implementation, this would find and damage enemy entities
        Debug.Log($"[IGNIS] Inferno deals {infernoDamagePerSecond * Time.deltaTime:F2} damage in {infernoRadius}m radius!");
    }
    
    /// <summary>
    /// Ends the Inferno Embrace ability.
    /// </summary>
    private void EndInfernoAbility()
    {
        isInfernoActive = false;
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("[IGNIS] Inferno Embrace fades. The flames subside.");
    }
    
    /// <summary>
    /// Checks if the Inferno Embrace ability is currently active.
    /// </summary>
    public bool IsInfernoActive()
    {
        return isInfernoActive;
    }
    
    /// <summary>
    /// Gets the remaining cooldown time for the ability.
    /// </summary>
    public float GetAbilityCooldownRemaining()
    {
        float elapsed = Time.time - lastAbilityUseTime;
        return Mathf.Max(0f, abilityCooldown - elapsed);
    }
}
