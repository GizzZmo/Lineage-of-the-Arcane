using UnityEngine;

/// <summary>
/// Base class for Tier 1 entities - The Scions.
/// Scions are descendants of the Parents with moderate power.
/// They have simpler temperament requirements and local environmental effects.
/// They inherit the Affinity system from MagicParent but gain affinity faster.
/// </summary>
public abstract class Scion : MagicParent
{
    [Header("Scion Configuration")]
    public string parentLineage;              // Which Parent they descend from
    public float localEffectRadius = 15f;     // Range of environmental effects
    public float scionAffinityBonus = 0.8f;   // Reduced base tether cost multiplier
    
    [Header("Scion Temperament")]
    public float temperamentTolerance = 5f;   // Seconds before temperament check
    public float punishmentMultiplier = 0.5f; // Reduced punishment compared to Parents
    
    [Header("Scion Affinity Bonuses")]
    [Tooltip("Scions grant bonus affinity to their parent lineage")]
    public float parentAffinityBonus = 0.5f;  // Bonus affinity added to parent entity
    
    protected override void ApplyEnvironmentalShift()
    {
        // Scions have local environmental effects rather than global
        ApplyLocalEnvironmentalShift();
    }
    
    /// <summary>
    /// Applies environmental effects within a limited radius.
    /// Override this to implement specific scion environmental effects.
    /// </summary>
    protected abstract void ApplyLocalEnvironmentalShift();
    
    /// <summary>
    /// Called when the scion is first summoned.
    /// </summary>
    public override void OnSummon(PlayerController player)
    {
        // Apply scion-specific tether cost reduction
        tetherCostPerSecond *= scionAffinityBonus;
        
        base.OnSummon(player);
        Debug.Log($"Scion {entityName} of the {parentLineage} lineage has answered the call.");
    }
    
    /// <summary>
    /// Override tether maintained to also grant bonus affinity to parent lineage.
    /// </summary>
    public override void OnTetherMaintained(float deltaTime)
    {
        base.OnTetherMaintained(deltaTime);
        
        // Scions grant bonus affinity to their parent lineage
        if (!string.IsNullOrEmpty(parentLineage))
        {
            string parentId = GetParentEntityId();
            if (!string.IsNullOrEmpty(parentId))
            {
                // Grant partial affinity to the parent entity as well
                AffinitySystem.Instance.AddTetherAffinity(parentId, deltaTime * parentAffinityBonus, isTemperamentSatisfied);
            }
        }
    }
    
    /// <summary>
    /// Gets the entity ID of the parent lineage.
    /// </summary>
    protected virtual string GetParentEntityId()
    {
        // Map lineage names to parent entity IDs
        switch (parentLineage)
        {
            case "Ignis Mater":
            case "Ignis":
                return "IgnisMater";
            case "Aqua Pater":
            case "Aqua":
                return "AquaPater";
            case "Terra Mater":
            case "Terra":
                return "TerraMater";
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Scions have simpler temperament requirements than Parents.
    /// </summary>
    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Scions are more forgiving with temperament checks
        PerformTemperamentCheck();
    }
    
    /// <summary>
    /// Override this to implement specific temperament checking logic.
    /// </summary>
    protected abstract void PerformTemperamentCheck();
    
    /// <summary>
    /// Applies reduced punishment compared to Parents.
    /// </summary>
    protected void ApplyPunishment(float baseDamage)
    {
        if (boundPlayer != null)
        {
            SetTemperamentSatisfied(false);
            float actualDamage = baseDamage * punishmentMultiplier;
            boundPlayer.TakeDamage(actualDamage);
            Debug.Log($"Scion {entityName} shows displeasure. ({actualDamage} damage)");
        }
    }
    
    /// <summary>
    /// Sets temperament as satisfied when the player is meeting requirements.
    /// Call this from derived classes when temperament checks pass.
    /// </summary>
    protected void ApproveTemperament()
    {
        SetTemperamentSatisfied(true);
    }
    
    public override void OnTetherBroken()
    {
        base.OnTetherBroken();
        // Scions have less severe rampant behavior
        Debug.Log($"Scion {entityName} fades with disappointment rather than rage.");
    }
}
