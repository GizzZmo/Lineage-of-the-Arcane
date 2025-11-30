using UnityEngine;

/// <summary>
/// Dolor Mater, The Suffering - The Pain Mother entity.
/// She demands sacrifice and suffering, rewarding players who take damage.
/// Her presence amplifies pain but also provides power through sacrifice.
/// 
/// Special Ability (Ascended): "Martyrdom" - Converts all damage taken into a powerful AoE attack.
/// </summary>
public class DolorMater : MagicParent
{
    [Header("Dolor Settings")]
    public float sacrificeInterval = 5.0f; // Seconds between required damage
    public float minimumSacrifice = 5.0f; // Minimum damage needed
    public float punishmentDamage = 8.0f;
    public Color ambientColor = new Color(0.5f, 0.1f, 0.2f); // Dark crimson
    public float damageAmplification = 1.3f; // All damage increased by 30%
    
    [Header("Special Ability - Martyrdom")]
    [Tooltip("Duration to collect damage for the explosion")]
    public float martyrdomDuration = 5f;
    [Tooltip("Damage multiplier for collected damage")]
    public float martyrdomMultiplier = 3f;
    [Tooltip("Radius of the martyrdom explosion")]
    public float martyrdomRadius = 12f;
    [Tooltip("Cooldown between ability uses")]
    public float abilityCooldown = 60f;
    
    private float lastSacrificeTime;
    private float damageAccumulated;
    private float lastAbilityUseTime = -999f;
    private bool isMartyrdomActive = false;
    private float martyrdomStartTime;
    private float collectedDamage;
    
    private void Start()
    {
        entityName = "Dolor Mater, The Suffering";
        entityId = "DolorMater";
        tetherCostPerSecond = 8.0f; // High cost
        
        // Configure rampant behavior specific to Dolor
        rampantBehavior = RampantBehavior.Vengeful; // Pain seeks the betrayer
        rampantDuration = 40f;
        rampantDamage = 22f; // Highest rampant damage
        
        ConfigureRampantState();
    }
    
    void Update()
    {
        // Update martyrdom ability if active
        if (isMartyrdomActive)
        {
            UpdateMartyrdomAbility();
        }
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Change lighting to dark crimson
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("Pain permeates the air. Dolor Mater demands your suffering.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: Dolor Mater demands the player take damage periodically
        // Check if player has taken damage recently
        float currentHealth = boundPlayer.currentHealth;
        
        // Track damage taken this interval
        if (Time.time - lastSacrificeTime > sacrificeInterval)
        {
            if (damageAccumulated < minimumSacrifice)
            {
                // Player hasn't suffered enough
                SetTemperamentSatisfied(false);
                PunishPlayer();
                Debug.Log("Dolor Mater is displeased. You must offer your pain!");
            }
            else
            {
                // Player has sacrificed enough
                SetTemperamentSatisfied(true);
                Debug.Log($"Dolor Mater accepts your offering of {damageAccumulated:F1} pain.");
            }
            
            // Reset for next interval
            damageAccumulated = 0f;
            lastSacrificeTime = Time.time;
        }
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
            Debug.LogWarning("[DOLOR] OnPlayerTakeDamage received negative damage value.");
            return;
        }
        
        damageAccumulated += damage;
        
        // If martyrdom is active, collect the damage
        if (isMartyrdomActive)
        {
            collectedDamage += damage;
            Debug.Log($"[DOLOR] Martyrdom collects {damage:F1} damage. Total: {collectedDamage:F1}");
        }
        
        Debug.Log($"Dolor Mater appreciates your sacrifice. ({damageAccumulated:F1}/{minimumSacrifice} this cycle)");
    }
    
    /// <summary>
    /// Gets the amplified damage amount for incoming damage.
    /// </summary>
    public float GetAmplifiedDamage(float baseDamage)
    {
        return baseDamage * damageAmplification;
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Dolor Mater inflicts pain upon you for your avoidance!");
        }
    }
    
    public override void OnTetherBroken()
    {
        isMartyrdomActive = false;
        
        base.OnTetherBroken();
        Debug.LogWarning("Dolor Mater's agony manifests! She seeks vengeance for the betrayal!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = ambientColor * 2f;
    }
    
    public override void OnSummon(PlayerController player)
    {
        base.OnSummon(player);
        
        // Initialize sacrifice tracking
        lastSacrificeTime = Time.time;
        damageAccumulated = 0f;
    }
    
    /// <summary>
    /// Called when the special ability becomes available.
    /// </summary>
    protected override void OnSpecialAbilityAvailable()
    {
        Debug.Log("[DOLOR] Martyrdom ability is now available! Press the ability key to activate.");
    }
    
    /// <summary>
    /// Activates the Martyrdom special ability.
    /// </summary>
    public override void ActivateSpecialAbility()
    {
        if (!hasSpecialAbility)
        {
            Debug.LogWarning("Martyrdom is not unlocked. Reach Ascended affinity with Dolor Mater.");
            return;
        }
        
        if (Time.time - lastAbilityUseTime < abilityCooldown)
        {
            float remaining = abilityCooldown - (Time.time - lastAbilityUseTime);
            Debug.Log($"[DOLOR] Martyrdom on cooldown. {remaining:F1}s remaining.");
            return;
        }
        
        if (boundPlayer == null)
        {
            Debug.LogWarning("Cannot activate ability without a tethered player.");
            return;
        }
        
        // Activate martyrdom collection phase
        isMartyrdomActive = true;
        martyrdomStartTime = Time.time;
        lastAbilityUseTime = Time.time;
        collectedDamage = 0f;
        
        Debug.Log("[DOLOR] MARTYRDOM ACTIVATED! All damage you take will be converted to power!");
        
        // Visual effect: Intense crimson ambient
        RenderSettings.ambientLight = new Color(0.8f, 0.1f, 0.2f);
    }
    
    /// <summary>
    /// Updates the Martyrdom ability while active.
    /// </summary>
    private void UpdateMartyrdomAbility()
    {
        if (Time.time - martyrdomStartTime >= martyrdomDuration)
        {
            TriggerMartyrdomExplosion();
            return;
        }
        
        float remaining = martyrdomDuration - (Time.time - martyrdomStartTime);
        Debug.Log($"[DOLOR] Martyrdom active. Take damage! {remaining:F1}s remaining. Collected: {collectedDamage:F1}");
    }
    
    /// <summary>
    /// Triggers the martyrdom explosion at the end of the collection phase.
    /// </summary>
    private void TriggerMartyrdomExplosion()
    {
        isMartyrdomActive = false;
        RenderSettings.ambientLight = ambientColor;
        
        float explosionDamage = collectedDamage * martyrdomMultiplier;
        
        if (explosionDamage > 0)
        {
            Debug.Log($"[DOLOR] MARTYRDOM EXPLOSION! {explosionDamage:F1} damage in {martyrdomRadius}m radius!");
            
            // In a full implementation, this would damage all enemies in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, martyrdomRadius);
            foreach (Collider col in colliders)
            {
                // Would damage enemies here
                // Placeholder for enemy damage system
            }
        }
        else
        {
            Debug.Log("[DOLOR] Martyrdom fizzles... No pain was collected.");
        }
        
        collectedDamage = 0f;
    }
    
    /// <summary>
    /// Checks if the Martyrdom ability is currently active.
    /// </summary>
    public bool IsMartyrdomActive()
    {
        return isMartyrdomActive;
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
    /// Gets the current damage accumulated this sacrifice cycle.
    /// </summary>
    public float GetDamageAccumulated()
    {
        return damageAccumulated;
    }
    
    /// <summary>
    /// Gets the current collected damage for martyrdom ability.
    /// </summary>
    public float GetCollectedDamage()
    {
        return collectedDamage;
    }
}
