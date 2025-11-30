using UnityEngine;

/// <summary>
/// Tempus Mater, The Eternal - The Time Mother entity.
/// She demands stillness and patience, punishing rushed actions.
/// Her presence slows time and creates temporal effects.
/// 
/// Special Ability (Ascended): "Temporal Stasis" - Freezes time for all entities except the player.
/// </summary>
public class TempusMater : MagicParent
{
    [Header("Tempus Settings")]
    public float stillnessThreshold = 4.0f; // Seconds of stillness required
    public float actionCooldown = 5.0f; // Time to wait between actions
    public float punishmentDamage = 5.0f;
    public Color ambientColor = new Color(0.6f, 0.5f, 0.8f); // Soft purple
    public float timeSlowMultiplier = 0.7f; // Slows time to 70%
    
    [Header("Special Ability - Temporal Stasis")]
    [Tooltip("Duration of the time freeze in seconds")]
    public float stasisDuration = 6f;
    [Tooltip("Time scale during stasis (near-freeze)")]
    public float stasisTimeScale = 0.1f;
    [Tooltip("Cooldown between ability uses")]
    public float abilityCooldown = 50f;
    
    private float lastActionTime;
    private float stillnessStartTime;
    private bool isStill = true;
    private float originalTimeScale;
    private float lastAbilityUseTime = -999f;
    private bool isStasisActive = false;
    private float stasisStartTime;
    
    private void Start()
    {
        entityName = "Tempus Mater, The Eternal";
        entityId = "TempusMater";
        tetherCostPerSecond = 9.0f; // High cost
        
        // Configure rampant behavior specific to Tempus
        rampantBehavior = RampantBehavior.Chaotic; // Time becomes unstable
        rampantDuration = 30f;
        rampantDamage = 16f;
        
        ConfigureRampantState();
    }
    
    void Update()
    {
        // Update stasis ability if active
        if (isStasisActive)
        {
            UpdateStasisAbility();
        }
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Change lighting to temporal purple tones
        RenderSettings.ambientLight = ambientColor;
        
        // Slow time globally
        originalTimeScale = Time.timeScale;
        Time.timeScale = timeSlowMultiplier;
        
        Debug.Log("Time slows to a crawl. Tempus Mater observes the stillness.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: Tempus Mater demands stillness and patience
        // Player should NOT attack frequently - opposite of Ignis
        bool hasActedRecently = (Time.time - boundPlayer.lastAttackTime) < actionCooldown;
        
        if (hasActedRecently)
        {
            // Player acted too quickly
            SetTemperamentSatisfied(false);
            PunishPlayer();
            isStill = false;
            stillnessStartTime = Time.time;
        }
        else
        {
            // Check if player has been still long enough
            if (!isStill)
            {
                if (Time.time - stillnessStartTime > stillnessThreshold)
                {
                    isStill = true;
                    SetTemperamentSatisfied(true);
                    Debug.Log("Tempus Mater approves of your patience.");
                }
            }
            else
            {
                SetTemperamentSatisfied(true);
            }
        }
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Tempus Mater ages you painfully for your haste!");
        }
    }
    
    public override void OnTetherBroken()
    {
        // Restore time scale before going rampant
        if (originalTimeScale > 0)
        {
            Time.timeScale = originalTimeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }
        
        isStasisActive = false;
        
        base.OnTetherBroken();
        Debug.LogWarning("Tempus Mater fractures time! Reality becomes unstable!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = ambientColor * 1.5f;
    }
    
    /// <summary>
    /// Called when Tempus Mater is dismissed properly.
    /// </summary>
    public void OnDismiss()
    {
        if (originalTimeScale > 0)
        {
            Time.timeScale = originalTimeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }
        
        RenderSettings.ambientLight = Color.white * 0.5f;
        Debug.Log("Time resumes its normal flow. Tempus Mater returns to eternity.");
    }
    
    /// <summary>
    /// Called when the special ability becomes available.
    /// </summary>
    protected override void OnSpecialAbilityAvailable()
    {
        Debug.Log("[TEMPUS] Temporal Stasis ability is now available! Press the ability key to activate.");
    }
    
    /// <summary>
    /// Activates the Temporal Stasis special ability.
    /// </summary>
    public override void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning("Temporal Stasis is not unlocked. Reach Ascended affinity with Tempus Mater.");
            return;
        }
        
        if (Time.time - lastAbilityUseTime < abilityCooldown)
        {
            float remaining = abilityCooldown - (Time.time - lastAbilityUseTime);
            Debug.Log($"[TEMPUS] Temporal Stasis on cooldown. {remaining:F1}s remaining.");
            return;
        }
        
        if (boundPlayer == null)
        {
            Debug.LogWarning("Cannot activate ability without a tethered player.");
            return;
        }
        
        // Activate temporal stasis
        isStasisActive = true;
        stasisStartTime = Time.time;
        lastAbilityUseTime = Time.time;
        
        Debug.Log("[TEMPUS] TEMPORAL STASIS ACTIVATED! Time freezes around you!");
        
        // Near-complete time freeze (player can still move due to game logic)
        // Note: In a full implementation, a TimeManager would coordinate time effects
        Time.timeScale = stasisTimeScale;
        
        // Visual effect: Deep purple ambient
        RenderSettings.ambientLight = new Color(0.4f, 0.2f, 0.6f);
    }
    
    /// <summary>
    /// Updates the Temporal Stasis ability while active.
    /// </summary>
    private void UpdateStasisAbility()
    {
        // Use unscaled time for ability duration since we modified timeScale
        if (Time.unscaledTime - stasisStartTime >= stasisDuration)
        {
            EndStasisAbility();
            return;
        }
        
        Debug.Log($"[TEMPUS] Time is frozen. {stasisDuration - (Time.unscaledTime - stasisStartTime):F1}s remaining.");
    }
    
    /// <summary>
    /// Ends the Temporal Stasis ability.
    /// </summary>
    private void EndStasisAbility()
    {
        isStasisActive = false;
        Time.timeScale = timeSlowMultiplier; // Return to slowed time, not normal
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("[TEMPUS] Temporal Stasis ends. Time begins to flow again.");
    }
    
    /// <summary>
    /// Checks if the Temporal Stasis ability is currently active.
    /// </summary>
    public bool IsStasisActive()
    {
        return isStasisActive;
    }
    
    /// <summary>
    /// Gets the remaining cooldown time for the ability.
    /// </summary>
    public float GetAbilityCooldownRemaining()
    {
        float elapsed = Time.time - lastAbilityUseTime;
        return Mathf.Max(0f, abilityCooldown - elapsed);
    }
    
    /// <summary>
    /// Gets the current stillness status.
    /// </summary>
    public bool IsStill()
    {
        return isStill;
    }
}
