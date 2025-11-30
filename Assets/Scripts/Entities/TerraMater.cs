using UnityEngine;

/// <summary>
/// Terra Mater, The Foundation - The Earth Mother entity.
/// She demands rhythmic actions and punishes inconsistency.
/// Her presence increases gravity and causes structures to emerge.
/// 
/// Special Ability (Ascended): "Earthen Bulwark" - Creates a protective barrier that absorbs damage.
/// </summary>
public class TerraMater : MagicParent
{
    [Header("Terra Settings")]
    public float rhythmWindow = 3.0f; // Expected interval between actions
    public float rhythmTolerance = 0.5f; // Acceptable deviation from rhythm
    public float rhythmTimeoutMultiplier = 2.0f; // Multiplier for timeout grace period
    public float punishmentDamage = 6.0f;
    public Color ambientColor = new Color(0.6f, 0.4f, 0.2f); // Earthy brown
    public float gravityMultiplier = 1.5f; // Increases gravity by 50%
    
    [Header("Special Ability - Earthen Bulwark")]
    [Tooltip("Duration of the barrier in seconds")]
    public float bulwarkDuration = 10f;
    [Tooltip("Maximum damage the barrier can absorb")]
    public float barrierHealth = 50f;
    [Tooltip("Cooldown between ability uses")]
    public float abilityCooldown = 60f;
    
    private float lastActionTime;
    private float originalGravity;
    private int rhythmStreak = 0; // Tracks consistent rhythmic actions
    private bool hasStartedRhythm = false;
    
    private float lastAbilityUseTime = -999f;
    private bool isBulwarkActive = false;
    private float bulwarkStartTime;
    private float currentBarrierHealth;
    
    private void Start()
    {
        entityName = "Terra Mater, The Foundation";
        entityId = "TerraMater";
        tetherCostPerSecond = 7.0f; // Moderate cost
        
        // Configure rampant behavior specific to Terra
        rampantBehavior = RampantBehavior.Destructive; // Earth destroys environment
        rampantDuration = 40f;
        rampantDamage = 18f;
        
        ConfigureRampantState();
    }
    
    void Update()
    {
        // Update bulwark ability if active
        if (isBulwarkActive)
        {
            UpdateBulwarkAbility();
        }
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to increase gravity and change lighting to earthy tones
        RenderSettings.ambientLight = ambientColor;
        originalGravity = Physics.gravity.y;
        Physics.gravity = new Vector3(0, originalGravity * gravityMultiplier, 0);
        Debug.Log("The earth trembles. Terra Mater's weight presses upon the world.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: Player must perform actions at a consistent rhythm
        // Check if the player has attacked and if it was rhythmic
        if (boundPlayer.lastAttackTime > lastActionTime)
        {
            float interval = boundPlayer.lastAttackTime - lastActionTime;
            
            if (hasStartedRhythm)
            {
                // Check if the interval is within acceptable rhythm tolerance
                float deviation = Mathf.Abs(interval - rhythmWindow);
                
                if (deviation <= rhythmTolerance)
                {
                    // Good rhythm!
                    rhythmStreak++;
                    SetTemperamentSatisfied(true);
                    Debug.Log($"Terra Mater approves of your rhythm. Streak: {rhythmStreak}");
                }
                else
                {
                    // Rhythm broken!
                    SetTemperamentSatisfied(false);
                    PunishPlayer();
                    rhythmStreak = 0;
                }
            }
            else
            {
                // First action establishes the rhythm
                hasStartedRhythm = true;
                Debug.Log("Terra Mater begins to measure your rhythm...");
            }
            
            lastActionTime = boundPlayer.lastAttackTime;
        }
        
        // Also punish if too much time passes without any action
        if (hasStartedRhythm && Time.time - lastActionTime > rhythmWindow + rhythmTolerance * rhythmTimeoutMultiplier)
        {
            SetTemperamentSatisfied(false);
            PunishPlayer();
            hasStartedRhythm = false;
            rhythmStreak = 0;
            Debug.Log("Terra Mater is displeased by your silence.");
        }
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Terra Mater shakes the ground beneath you!");
        }
    }
    
    public override void OnTetherBroken()
    {
        // Restore gravity before going rampant
        if (originalGravity != 0)
        {
            Physics.gravity = new Vector3(0, originalGravity, 0);
        }
        
        isBulwarkActive = false;
        
        base.OnTetherBroken();
        Debug.LogWarning("Terra Mater erupts in fury! The ground cracks and splits!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = ambientColor * 1.5f;
        
        // Extreme gravity during rampant
        Physics.gravity = new Vector3(0, originalGravity * 2f, 0);
    }
    
    /// <summary>
    /// Called when Terra Mater is dismissed properly.
    /// </summary>
    public void OnDismiss()
    {
        if (originalGravity != 0)
        {
            Physics.gravity = new Vector3(0, originalGravity, 0);
        }
        RenderSettings.ambientLight = Color.white * 0.5f;
        Debug.Log("The earth settles. Terra Mater returns to her slumber.");
    }
    
    /// <summary>
    /// Gets the current rhythm streak count.
    /// </summary>
    public int GetRhythmStreak()
    {
        return rhythmStreak;
    }
    
    /// <summary>
    /// Bonus effect when rhythm is maintained well.
    /// </summary>
    public void ApplyRhythmBonus()
    {
        if (rhythmStreak >= 5 && boundPlayer != null)
        {
            // Reduce tether cost for good rhythm
            float reduction = tetherCostPerSecond * 0.1f * (rhythmStreak / 5);
            Debug.Log($"Terra Mater rewards your rhythm with reduced tether cost: {reduction}");
        }
    }
    
    /// <summary>
    /// Called when the special ability becomes available.
    /// </summary>
    protected override void OnSpecialAbilityAvailable()
    {
        Debug.Log("[TERRA] Earthen Bulwark ability is now available! Press the ability key to activate.");
    }
    
    /// <summary>
    /// Activates the Earthen Bulwark special ability.
    /// </summary>
    public override void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning("Earthen Bulwark is not unlocked. Reach Ascended affinity with Terra Mater.");
            return;
        }
        
        if (Time.time - lastAbilityUseTime < abilityCooldown)
        {
            float remaining = abilityCooldown - (Time.time - lastAbilityUseTime);
            Debug.Log($"[TERRA] Earthen Bulwark on cooldown. {remaining:F1}s remaining.");
            return;
        }
        
        if (boundPlayer == null)
        {
            Debug.LogWarning("Cannot activate ability without a tethered player.");
            return;
        }
        
        // Create the barrier
        isBulwarkActive = true;
        bulwarkStartTime = Time.time;
        currentBarrierHealth = barrierHealth;
        lastAbilityUseTime = Time.time;
        
        Debug.Log("[TERRA] EARTHEN BULWARK ACTIVATED! Stone shields surround you!");
        
        // Visual effect: Darken ambient with earthy tones
        RenderSettings.ambientLight = new Color(0.4f, 0.3f, 0.2f);
    }
    
    /// <summary>
    /// Updates the Earthen Bulwark ability while active.
    /// </summary>
    private void UpdateBulwarkAbility()
    {
        if (Time.time - bulwarkStartTime >= bulwarkDuration || currentBarrierHealth <= 0)
        {
            EndBulwarkAbility();
            return;
        }
        
        // The barrier absorbs damage - would need to hook into damage system
        Debug.Log($"[TERRA] Bulwark active. Barrier health: {currentBarrierHealth:F1}/{barrierHealth}");
    }
    
    /// <summary>
    /// Called when the barrier absorbs damage.
    /// </summary>
    /// <param name="damage">The amount of damage to absorb.</param>
    /// <returns>The amount of damage that passed through the barrier.</returns>
    public float AbsorbDamage(float damage)
    {
        if (!isBulwarkActive) return damage;
        
        if (damage <= currentBarrierHealth)
        {
            currentBarrierHealth -= damage;
            Debug.Log($"[TERRA] Bulwark absorbs {damage:F1} damage. Remaining: {currentBarrierHealth:F1}");
            return 0f;
        }
        else
        {
            float passthrough = damage - currentBarrierHealth;
            currentBarrierHealth = 0;
            Debug.Log($"[TERRA] Bulwark breaks! {passthrough:F1} damage passes through.");
            EndBulwarkAbility();
            return passthrough;
        }
    }
    
    /// <summary>
    /// Ends the Earthen Bulwark ability.
    /// </summary>
    private void EndBulwarkAbility()
    {
        isBulwarkActive = false;
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("[TERRA] Earthen Bulwark crumbles. The stone returns to earth.");
    }
    
    /// <summary>
    /// Checks if the Earthen Bulwark ability is currently active.
    /// </summary>
    public bool IsBulwarkActive()
    {
        return isBulwarkActive;
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
    /// Gets the current barrier health remaining.
    /// </summary>
    public float GetBarrierHealth()
    {
        return currentBarrierHealth;
    }
}
