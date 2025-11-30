using UnityEngine;

/// <summary>
/// Base class for Tier 2 entities - The Heirs.
/// Heirs are the weakest magical entities with forgiving temperaments.
/// They have minimal environmental effects and low tether costs.
/// Heirs build affinity very quickly due to their gentle nature.
/// </summary>
public abstract class Heir : MagicParent
{
    [Header("Heir Configuration")]
    public string ancestralLine;              // The lineage they belong to
    public float effectRange = 5f;            // Very limited effect range
    public float heirAffinityBonus = 0.5f;    // Significantly reduced tether cost
    
    [Header("Heir Temperament")]
    public float temperamentGracePeriod = 10f;  // Long grace period before any check
    public float punishmentMultiplier = 0.2f;   // Very mild punishment
    public bool isForgiving = true;             // Heirs rarely punish
    
    [Header("Growth System")]
    public float bondStrength = 0f;             // Increases with successful tethering
    public float maxBondStrength = 100f;        // Maximum bond level
    public float bondGrowthRate = 1f;           // How fast bond grows per second
    
    [Header("Heir Affinity Bonuses")]
    [Tooltip("Heirs grant bonus affinity to their ancestral lineage")]
    public float ancestralAffinityBonus = 0.3f; // Bonus affinity added to parent and scion
    [Tooltip("Multiplier for affinity gain rate (Heirs bond faster)")]
    public float affinityGainMultiplier = 1.5f;
    
    protected override void ApplyEnvironmentalShift()
    {
        // Heirs have minimal environmental effects
        ApplyMinimalEnvironmentalShift();
    }
    
    /// <summary>
    /// Applies very subtle environmental effects.
    /// Override this to implement specific heir environmental effects.
    /// </summary>
    protected abstract void ApplyMinimalEnvironmentalShift();
    
    /// <summary>
    /// Called when the heir is summoned.
    /// </summary>
    public override void OnSummon(PlayerController player)
    {
        // Apply significant affinity bonus
        tetherCostPerSecond *= heirAffinityBonus;
        
        base.OnSummon(player);
        Debug.Log($"Heir {entityName} of the {ancestralLine} bloodline emerges timidly.");
    }
    
    protected virtual void Update()
    {
        // Grow bond strength while tethered
        if (boundPlayer != null && bondStrength < maxBondStrength)
        {
            GrowBond();
        }
    }
    
    /// <summary>
    /// Override tether maintained to add bonus affinity and faster growth.
    /// Note: We do NOT call base.OnTetherMaintained() because we want complete control
    /// over the affinity calculation for Heirs (using the multiplier instead of additive).
    /// </summary>
    public override void OnTetherMaintained(float deltaTime)
    {
        // Heirs give extra affinity due to their gentle nature
        // Use multiplied time instead of base implementation to avoid double tracking
        float bonusTime = deltaTime * affinityGainMultiplier;
        AffinitySystem.Instance.AddTetherAffinity(entityId, bonusTime, isTemperamentSatisfied);
        
        // Also grant bonus affinity to the ancestral lineage
        if (!string.IsNullOrEmpty(ancestralLine))
        {
            string parentId = GetAncestralParentId();
            if (!string.IsNullOrEmpty(parentId))
            {
                AffinitySystem.Instance.AddTetherAffinity(parentId, deltaTime * ancestralAffinityBonus, true);
            }
        }
    }
    
    /// <summary>
    /// Gets the entity ID of the ancestral parent.
    /// </summary>
    protected virtual string GetAncestralParentId()
    {
        // Map ancestral line names to parent entity IDs
        switch (ancestralLine)
        {
            case "Ignis":
            case "Fire":
                return "IgnisMater";
            case "Aqua":
            case "Water":
                return "AquaPater";
            case "Terra":
            case "Earth":
                return "TerraMater";
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Increases the bond strength with the player.
    /// </summary>
    protected void GrowBond()
    {
        bondStrength += bondGrowthRate * Time.deltaTime;
        bondStrength = Mathf.Min(bondStrength, maxBondStrength);
        
        // Reduce tether cost as bond grows
        float bondBonus = 1f - (bondStrength / maxBondStrength * 0.5f);
        // Tether cost reduction applied through affinity system
    }
    
    /// <summary>
    /// Heirs have very forgiving temperament requirements.
    /// </summary>
    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Heirs are always temperament-satisfied by default
        SetTemperamentSatisfied(true);
        
        if (isForgiving)
        {
            // Heirs only check temperament after a long grace period
            PerformGentleTemperamentCheck();
        }
        else
        {
            PerformTemperamentCheck();
        }
    }
    
    /// <summary>
    /// A gentle temperament check that rarely results in punishment.
    /// </summary>
    protected virtual void PerformGentleTemperamentCheck()
    {
        // By default, heirs are very lenient
        // Override this for specific heir behavior
    }
    
    /// <summary>
    /// Override this to implement specific temperament checking logic.
    /// </summary>
    protected abstract void PerformTemperamentCheck();
    
    /// <summary>
    /// Applies very mild punishment.
    /// </summary>
    protected void ApplyMildPunishment(float baseDamage)
    {
        if (boundPlayer != null && !isForgiving)
        {
            SetTemperamentSatisfied(false);
            float actualDamage = baseDamage * punishmentMultiplier;
            boundPlayer.TakeDamage(actualDamage);
            Debug.Log($"Heir {entityName} is upset. ({actualDamage} damage)");
        }
        else
        {
            Debug.Log($"Heir {entityName} looks sad but forgives you.");
        }
    }
    
    /// <summary>
    /// Called when the tether breaks.
    /// NOTE: Heirs intentionally do NOT call base.OnTetherBroken() because:
    /// 1. Heirs are too weak to enter a rampant state
    /// 2. They simply fade away instead of becoming hostile
    /// 3. This prevents the RampantState from being triggered
    /// However, we do record a reduced betrayal in the affinity system.
    /// </summary>
    public override void OnTetherBroken()
    {
        // Heirs don't go rampant - they simply fade away
        Debug.Log($"Heir {entityName} fades away with a sad expression...");
        
        // Record a smaller betrayal penalty for heirs using the proper AffinitySystem method
        // Heirs are forgiving, so the penalty is only 5 instead of the default 15
        AffinitySystem.Instance.OnTetherBetrayalWithPenalty(entityId, 5f);
        
        // Intentionally NOT calling base.OnTetherBroken() - see method summary
    }
    
    /// <summary>
    /// Called when the tether is cleanly severed.
    /// Heirs give extra affinity bonus when dismissed properly.
    /// </summary>
    public override void OnTetherSeveredCleanly()
    {
        // Use the AffinitySystem method that includes a bonus (3 extra affinity for heirs)
        // This handles all the proper event firing and level updates
        AffinitySystem.Instance.OnSuccessfulTetherWithBonus(entityId, 3f);
        
        // Clear the bound player reference (same as base would do)
        boundPlayer = null;
        
        Debug.Log($"Heir {entityName} waves goodbye warmly, grateful for the gentle parting.");
    }
    
    /// <summary>
    /// Returns the current bond level as a percentage.
    /// </summary>
    public float GetBondPercentage()
    {
        return (bondStrength / maxBondStrength) * 100f;
    }
    
    /// <summary>
    /// Checks if the bond is strong enough for evolution.
    /// </summary>
    public bool CanEvolve()
    {
        return bondStrength >= maxBondStrength;
    }
}
