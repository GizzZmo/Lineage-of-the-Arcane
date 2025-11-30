using UnityEngine;

/// <summary>
/// Aqua Pater, The Depths - The Water Father entity.
/// He demands passivity and punishes aggression.
/// His presence floods the world and slows movement.
/// </summary>
public class AquaPater : MagicParent
{
    [Header("Aqua Settings")]
    public float aggressionThreshold = 2.0f; // Seconds after attacking before punishment ends
    public float punishmentDamage = 4.0f;
    public Color ambientColor = new Color(0.2f, 0.4f, 0.8f); // Deep blue
    public float movementSlowMultiplier = 0.6f; // Slows movement by 40%
    
    private float lastAggressionTime;
    private float originalMoveSpeed;
    
    private void Start()
    {
        entityName = "Aqua Pater, The Depths";
        tetherCostPerSecond = 8.0f; // Moderate-high cost
        
        // Configure rampant behavior specific to Aqua
        rampantBehavior = RampantBehavior.Chaotic; // Water moves unpredictably
        rampantDuration = 35f;
        rampantDamage = 15f;
        
        ConfigureRampantState();
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
            if (Time.time - lastAggressionTime > 1.0f) // Punish every second during aggression
            {
                PunishPlayer();
                lastAggressionTime = Time.time;
            }
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
}
