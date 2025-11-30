using UnityEngine;

/// <summary>
/// Ember Scion - A Tier 1 descendant of Ignis Mater.
/// Less demanding than Ignis Mater but still requires aggression.
/// Tethering with Ember also builds affinity with Ignis Mater.
/// </summary>
public class EmberScion : Scion
{
    [Header("Ember Settings")]
    public float hesitationThreshold = 5.0f; // More lenient than Ignis Mater
    public float basePunishmentDamage = 3.0f;
    public Color localAmbientColor = new Color(1f, 0.5f, 0.2f); // Orange glow
    
    private float lastTemperamentCheckTime;
    
    private void Start()
    {
        entityName = "Ember, The Spark";
        entityId = "EmberScion";
        parentLineage = "Ignis Mater";
        tetherCostPerSecond = 5.0f; // Lower base cost than Ignis
        
        // Configure less severe rampant behavior
        rampantBehavior = RampantBehavior.Chaotic; // Less focused than parent
        rampantDuration = 15f; // Shorter duration
        rampantDamage = 10f;
        
        ConfigureRampantState();
    }
    
    protected override void ApplyLocalEnvironmentalShift()
    {
        // Apply a localized warm glow effect
        // In full implementation, this would use a point light or area effect
        Debug.Log("A warm glow emanates from Ember. The nearby air shimmers with heat.");
        
        // Subtle ambient light change (less dramatic than Ignis)
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, localAmbientColor, 0.3f);
    }
    
    protected override void PerformTemperamentCheck()
    {
        if (boundPlayer == null) return;
        
        // Only check periodically (more tolerant)
        if (Time.time - lastTemperamentCheckTime < temperamentTolerance)
        {
            return;
        }
        
        lastTemperamentCheckTime = Time.time;
        
        // Check if player hasn't attacked in threshold seconds
        if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
        {
            ApplyPunishment(basePunishmentDamage);
            Debug.Log("Ember flickers with disappointment at your hesitation.");
        }
        else
        {
            ApproveTemperament();
        }
    }
    
    public override void OnTetherBroken()
    {
        base.OnTetherBroken();
        Debug.Log("Ember sputters and sparks wildly before fading!");
        
        // Reset ambient light effect
        RenderSettings.ambientLight = Color.white * 0.5f;
    }
}
