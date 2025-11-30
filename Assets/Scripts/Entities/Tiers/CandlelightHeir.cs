using UnityEngine;

/// <summary>
/// Candlelight Heir - A Tier 2 descendant of the Ignis lineage.
/// Very forgiving and builds bond with the player over time.
/// Tethering with Candlelight also builds affinity with Ignis Mater and Ember Scion.
/// </summary>
public class CandlelightHeir : Heir
{
    [Header("Candlelight Settings")]
    public float warmthRadius = 3f;
    public Color candleGlow = new Color(1f, 0.8f, 0.4f); // Soft warm glow
    public float flickerSpeed = 2f;
    
    private float lastCheckTime;
    private Light pointLight;
    
    private void Start()
    {
        entityName = "Candlelight, The Gentle Flame";
        entityId = "CandlelightHeir";
        ancestralLine = "Ignis";
        tetherCostPerSecond = 2.0f; // Very low base cost
        
        // Heirs don't go rampant - they fade away
        rampantBehavior = RampantBehavior.Chaotic;
        rampantDuration = 5f; // Very short if it happens at all
        rampantDamage = 2f;   // Minimal damage
        
        // Set heir-specific properties
        isForgiving = true;
        bondGrowthRate = 2f; // Bonds quickly with the player
        
        ConfigureRampantState();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Candlelight flickers gently
        UpdateFlicker();
    }
    
    void UpdateFlicker()
    {
        if (pointLight != null)
        {
            // Gentle flickering effect
            float flicker = 1f + Mathf.Sin(Time.time * flickerSpeed) * 0.1f;
            pointLight.intensity = flicker;
        }
    }
    
    protected override void ApplyMinimalEnvironmentalShift()
    {
        // Create a soft point light effect
        Debug.Log("A gentle warmth spreads from Candlelight. You feel comforted.");
        
        CreateOrUpdateLight();
    }
    
    /// <summary>
    /// Creates or updates the Light component for the candle effect.
    /// The Light component is created at runtime to provide the warm glow.
    /// </summary>
    void CreateOrUpdateLight()
    {
        // Get or create the Light component
        if (pointLight == null)
        {
            pointLight = GetComponent<Light>();
            if (pointLight == null)
            {
                pointLight = gameObject.AddComponent<Light>();
            }
        }
        
        // Configure the light for a candle-like effect
        pointLight.type = LightType.Point;
        pointLight.color = candleGlow;
        pointLight.range = warmthRadius;
        pointLight.intensity = 1f;
        
        Debug.Log("Candlelight illuminates the area with a soft glow.");
    }
    
    protected override void PerformTemperamentCheck()
    {
        // Candlelight has no strict temperament requirements
        // It simply appreciates being kept active
        
        if (boundPlayer == null) return;
        
        // Only check very infrequently
        if (Time.time - lastCheckTime < temperamentGracePeriod)
        {
            return;
        }
        
        lastCheckTime = Time.time;
        
        // Candlelight just wants some attention occasionally
        // No punishment, just a gentle reminder
        Debug.Log("Candlelight flickers gently, hoping for some interaction.");
    }
    
    protected override void PerformGentleTemperamentCheck()
    {
        // Candlelight is very forgiving - no actual checks needed
        // Just grows bond over time
    }
    
    public override void OnTetherBroken()
    {
        // Heirs don't trigger rampant state - they just fade
        Debug.Log("Candlelight flickers sadly and extinguishes. A small wisp of smoke rises...");
        
        // Could trigger a small visual effect here
        if (pointLight != null)
        {
            pointLight.enabled = false;
        }
        
        // Call base to handle the reduced affinity penalty
        base.OnTetherBroken();
    }
    
    /// <summary>
    /// Provides a small healing effect when bond is strong.
    /// </summary>
    public void ProvideSolace()
    {
        if (boundPlayer != null && bondStrength > 50f)
        {
            float healAmount = bondStrength / maxBondStrength * 2f;
            boundPlayer.currentHealth = Mathf.Min(
                boundPlayer.currentHealth + healAmount,
                boundPlayer.maxHealth
            );
            Debug.Log($"Candlelight's warmth heals you for {healAmount} health.");
        }
    }
}
