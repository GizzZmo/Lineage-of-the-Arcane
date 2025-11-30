using UnityEngine;

/// <summary>
/// Aqua Pater, The Depths - The Water Father entity.
/// He demands passivity and punishes aggression.
/// His presence floods the world and slows movement.
/// 
/// Special Ability (Ascended): "Tidal Sanctuary" - Creates a healing zone that restores health over time.
/// </summary>
public class AquaPater : MagicParent
{
    [Header("Aqua Settings")]
    public float aggressionThreshold = 2.0f; // Seconds after attacking before punishment ends
    public float punishmentDamage = 4.0f;
    public Color ambientColor = new Color(0.2f, 0.4f, 0.8f); // Deep blue
    public float movementSlowMultiplier = 0.6f; // Slows movement by 40%
    
    [Header("Special Ability - Tidal Sanctuary")]
    [Tooltip("Duration of the healing zone in seconds")]
    public float sanctuaryDuration = 8f;
    [Tooltip("Health restored per second while in sanctuary")]
    public float healingPerSecond = 5f;
    [Tooltip("Radius of the sanctuary zone")]
    public float sanctuaryRadius = 6f;
    [Tooltip("Cooldown between ability uses")]
    public float abilityCooldown = 45f;
    
    private float lastAggressionTime;
    private float originalMoveSpeed;
    private float lastAbilityUseTime = -999f;
    private bool isSanctuaryActive = false;
    private float sanctuaryStartTime;
    private Vector3 sanctuaryPosition;
    
    private void Start()
    {
        entityName = "Aqua Pater, The Depths";
        entityId = "AquaPater";
        tetherCostPerSecond = 8.0f; // Moderate-high cost
        
        // Configure rampant behavior specific to Aqua
        rampantBehavior = RampantBehavior.Chaotic; // Water moves unpredictably
        rampantDuration = 35f;
        rampantDamage = 15f;
        
        ConfigureRampantState();
    }
    
    void Update()
    {
        // Update sanctuary ability if active
        if (isSanctuaryActive)
        {
            UpdateSanctuaryAbility();
        }
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to flood the world and make lighting blue
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("The world floods with deep waters. Aqua Pater watches from below.");
        
        // Slow player movement
        if (boundPlayer != null)
        {
            originalMoveSpeed = boundPlayer.moveSpeed;
            boundPlayer.moveSpeed *= movementSlowMultiplier;
            Debug.Log("Your movements become sluggish in the water.");
        }
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: If player has attacked recently, punish them
        // Aqua Pater demands passivity - no combat
        if (Time.time - boundPlayer.lastAttackTime < aggressionThreshold)
        {
            SetTemperamentSatisfied(false);
            if (Time.time - lastAggressionTime > 1.0f) // Punish every second during aggression
            {
                PunishPlayer();
                lastAggressionTime = Time.time;
            }
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
            Debug.Log("Aqua Pater drowns you for your violence!");
        }
    }
    
    public override void OnTetherBroken()
    {
        // Restore player movement speed before going rampant
        if (boundPlayer != null && originalMoveSpeed > 0)
        {
            boundPlayer.moveSpeed = originalMoveSpeed;
        }
        
        isSanctuaryActive = false;
        
        base.OnTetherBroken();
        Debug.LogWarning("Aqua Pater surges in fury! A tidal wave approaches!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = ambientColor * 1.5f;
    }
    
    /// <summary>
    /// Called when Aqua Pater is dismissed properly.
    /// </summary>
    public void OnDismiss()
    {
        if (boundPlayer != null && originalMoveSpeed > 0)
        {
            boundPlayer.moveSpeed = originalMoveSpeed;
        }
        RenderSettings.ambientLight = Color.white * 0.5f;
        Debug.Log("The waters recede. Aqua Pater returns to the depths.");
    }
    
    /// <summary>
    /// Called when the special ability becomes available.
    /// </summary>
    protected override void OnSpecialAbilityAvailable()
    {
        Debug.Log("[AQUA] Tidal Sanctuary ability is now available! Press the ability key to activate.");
    }
    
    /// <summary>
    /// Activates the Tidal Sanctuary special ability.
    /// </summary>
    public override void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning("Tidal Sanctuary is not unlocked. Reach Ascended affinity with Aqua Pater.");
            return;
        }
        
        if (Time.time - lastAbilityUseTime < abilityCooldown)
        {
            float remaining = abilityCooldown - (Time.time - lastAbilityUseTime);
            Debug.Log($"[AQUA] Tidal Sanctuary on cooldown. {remaining:F1}s remaining.");
            return;
        }
        
        if (boundPlayer == null)
        {
            Debug.LogWarning("Cannot activate ability without a tethered player.");
            return;
        }
        
        // Create the sanctuary at player's position
        isSanctuaryActive = true;
        sanctuaryStartTime = Time.time;
        sanctuaryPosition = boundPlayer.transform.position;
        lastAbilityUseTime = Time.time;
        
        Debug.Log("[AQUA] TIDAL SANCTUARY ACTIVATED! Healing waters surround you!");
        
        // Visual effect: Intensify ambient with calming blue
        RenderSettings.ambientLight = new Color(0.3f, 0.6f, 1f);
    }
    
    /// <summary>
    /// Updates the Tidal Sanctuary ability while active.
    /// </summary>
    private void UpdateSanctuaryAbility()
    {
        if (Time.time - sanctuaryStartTime >= sanctuaryDuration)
        {
            EndSanctuaryAbility();
            return;
        }
        
        // Heal player if within sanctuary radius
        if (boundPlayer != null)
        {
            float distance = Vector3.Distance(boundPlayer.transform.position, sanctuaryPosition);
            if (distance <= sanctuaryRadius)
            {
                float healAmount = healingPerSecond * Time.deltaTime;
                boundPlayer.currentHealth = Mathf.Min(
                    boundPlayer.currentHealth + healAmount,
                    boundPlayer.maxHealth
                );
                Debug.Log($"[AQUA] Sanctuary heals for {healAmount:F2}. Health: {boundPlayer.currentHealth:F1}");
            }
        }
    }
    
    /// <summary>
    /// Ends the Tidal Sanctuary ability.
    /// </summary>
    private void EndSanctuaryAbility()
    {
        isSanctuaryActive = false;
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("[AQUA] Tidal Sanctuary fades. The healing waters recede.");
    }
    
    /// <summary>
    /// Checks if the Tidal Sanctuary ability is currently active.
    /// </summary>
    public bool IsSanctuaryActive()
    {
        return isSanctuaryActive;
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
