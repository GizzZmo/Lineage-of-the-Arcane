using UnityEngine;
using System.Collections;

/// <summary>
/// Manages visual effects that change based on affinity level with entities.
/// Entities at higher affinity levels appear differently to reflect the bond.
/// </summary>
public class AffinityVisualEffect : MonoBehaviour
{
    [Header("Entity Reference")]
    [Tooltip("The MagicParent entity this visual effect is attached to")]
    public MagicParent entity;
    
    [Header("Glow Settings")]
    [Tooltip("Material to use for the glow effect")]
    public Material glowMaterial;
    
    [Tooltip("Base glow intensity at Stranger level")]
    public float baseGlowIntensity = 0.5f;
    
    [Tooltip("Maximum glow intensity at Ascended level")]
    public float maxGlowIntensity = 3.0f;
    
    [Header("Scale Settings")]
    [Tooltip("Whether the entity scales with affinity")]
    public bool scaleWithAffinity = true;
    
    [Tooltip("Base scale at Stranger level")]
    public float baseScale = 1.0f;
    
    [Tooltip("Scale bonus at Ascended level")]
    public float ascendedScaleBonus = 0.3f;
    
    [Header("Particle Settings")]
    [Tooltip("Particle system for ambient effects")]
    public ParticleSystem ambientParticles;
    
    [Tooltip("Base emission rate")]
    public float baseEmissionRate = 5f;
    
    [Tooltip("Maximum emission rate at Ascended level")]
    public float maxEmissionRate = 50f;
    
    [Header("Color Settings")]
    [Tooltip("Colors for each affinity level")]
    public Color hostileColor = new Color(0.5f, 0f, 0f);
    public Color strangerColor = Color.white;
    public Color acquaintedColor = new Color(0.9f, 0.9f, 0.5f);
    public Color bondedColor = new Color(0.5f, 0.9f, 0.5f);
    public Color devotedColor = new Color(0.5f, 0.7f, 1f);
    public Color ascendedColor = new Color(1f, 0.85f, 0f);
    
    [Header("Aura Settings")]
    [Tooltip("Light component for aura effect")]
    public Light auraLight;
    
    [Tooltip("Base aura range")]
    public float baseAuraRange = 2f;
    
    [Tooltip("Maximum aura range at Ascended level")]
    public float maxAuraRange = 8f;
    
    [Header("Animation Settings")]
    [Tooltip("Speed of visual transitions")]
    public float transitionSpeed = 2f;
    
    [Tooltip("Enable pulse effect at high affinity")]
    public bool enablePulseEffect = true;
    
    [Tooltip("Pulse speed for Devoted/Ascended levels")]
    public float pulseSpeed = 1.5f;
    
    private Renderer entityRenderer;
    private Vector3 originalScale;
    private float currentGlowIntensity;
    private float targetGlowIntensity;
    private Color currentColor;
    private Color targetColor;
    private AffinityLevel lastKnownLevel;
    private MaterialPropertyBlock propertyBlock;
    
    void Start()
    {
        // Get renderer component
        entityRenderer = GetComponent<Renderer>();
        if (entityRenderer == null)
        {
            entityRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Store original scale
        originalScale = transform.localScale;
        
        // Initialize property block for material changes
        propertyBlock = new MaterialPropertyBlock();
        
        // Initialize values
        currentGlowIntensity = baseGlowIntensity;
        targetGlowIntensity = baseGlowIntensity;
        currentColor = strangerColor;
        targetColor = strangerColor;
        lastKnownLevel = AffinityLevel.Stranger;
        
        // Create aura light if not assigned
        if (auraLight == null)
        {
            CreateAuraLight();
        }
        
        // Subscribe to affinity events
        if (entity != null)
        {
            AffinitySystem.Instance.OnAffinityLevelChanged += OnAffinityLevelChanged;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        AffinitySystem instance = FindObjectOfType<AffinitySystem>();
        if (instance != null)
        {
            instance.OnAffinityLevelChanged -= OnAffinityLevelChanged;
        }
    }
    
    void Update()
    {
        if (entity == null) return;
        
        // Get current affinity level
        AffinityLevel currentLevel = entity.GetAffinityLevel();
        
        // Update targets if level changed
        if (currentLevel != lastKnownLevel)
        {
            UpdateTargetValues(currentLevel);
            lastKnownLevel = currentLevel;
        }
        
        // Smoothly transition visual effects
        UpdateVisuals();
        
        // Apply pulse effect for high affinity levels
        if (enablePulseEffect && (currentLevel == AffinityLevel.Devoted || currentLevel == AffinityLevel.Ascended))
        {
            ApplyPulseEffect(currentLevel);
        }
    }
    
    /// <summary>
    /// Creates a light component for the aura effect.
    /// </summary>
    private void CreateAuraLight()
    {
        GameObject lightObj = new GameObject("AffinityAuraLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        auraLight = lightObj.AddComponent<Light>();
        auraLight.type = LightType.Point;
        auraLight.range = baseAuraRange;
        auraLight.intensity = 0.5f;
        auraLight.color = strangerColor;
    }
    
    /// <summary>
    /// Updates target values based on affinity level.
    /// </summary>
    private void UpdateTargetValues(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                targetGlowIntensity = baseGlowIntensity * 0.5f;
                targetColor = hostileColor;
                break;
            case AffinityLevel.Stranger:
                targetGlowIntensity = baseGlowIntensity;
                targetColor = strangerColor;
                break;
            case AffinityLevel.Acquainted:
                targetGlowIntensity = Mathf.Lerp(baseGlowIntensity, maxGlowIntensity, 0.25f);
                targetColor = acquaintedColor;
                break;
            case AffinityLevel.Bonded:
                targetGlowIntensity = Mathf.Lerp(baseGlowIntensity, maxGlowIntensity, 0.5f);
                targetColor = bondedColor;
                break;
            case AffinityLevel.Devoted:
                targetGlowIntensity = Mathf.Lerp(baseGlowIntensity, maxGlowIntensity, 0.75f);
                targetColor = devotedColor;
                break;
            case AffinityLevel.Ascended:
                targetGlowIntensity = maxGlowIntensity;
                targetColor = ascendedColor;
                break;
        }
        
        Debug.Log($"[VISUAL] Affinity visual updating to {level} level");
    }
    
    /// <summary>
    /// Smoothly updates visual effects towards target values.
    /// </summary>
    private void UpdateVisuals()
    {
        // Smooth transitions
        currentGlowIntensity = Mathf.Lerp(currentGlowIntensity, targetGlowIntensity, Time.deltaTime * transitionSpeed);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        
        // Apply glow to material
        if (entityRenderer != null && glowMaterial != null)
        {
            entityRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_EmissionIntensity", currentGlowIntensity);
            propertyBlock.SetColor("_EmissionColor", currentColor * currentGlowIntensity);
            entityRenderer.SetPropertyBlock(propertyBlock);
        }
        
        // Update aura light
        if (auraLight != null)
        {
            auraLight.color = currentColor;
            auraLight.intensity = currentGlowIntensity * 0.5f;
            
            float targetRange = GetAuraRangeForLevel(lastKnownLevel);
            auraLight.range = Mathf.Lerp(auraLight.range, targetRange, Time.deltaTime * transitionSpeed);
        }
        
        // Update scale
        if (scaleWithAffinity)
        {
            float targetScaleMultiplier = GetScaleMultiplierForLevel(lastKnownLevel);
            Vector3 targetScale = originalScale * targetScaleMultiplier;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        }
        
        // Update particle emission
        if (ambientParticles != null)
        {
            var emission = ambientParticles.emission;
            float targetRate = GetEmissionRateForLevel(lastKnownLevel);
            emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, targetRate, Time.deltaTime * transitionSpeed);
            
            var main = ambientParticles.main;
            main.startColor = currentColor;
        }
    }
    
    /// <summary>
    /// Applies a pulsing effect for high affinity levels.
    /// </summary>
    private void ApplyPulseEffect(AffinityLevel level)
    {
        float pulseMultiplier = level == AffinityLevel.Ascended ? 1.5f : 1.2f;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f * pulseMultiplier;
        
        if (auraLight != null)
        {
            auraLight.intensity = currentGlowIntensity * 0.5f * pulse;
        }
        
        if (entityRenderer != null && glowMaterial != null)
        {
            entityRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_EmissionIntensity", currentGlowIntensity * pulse);
            entityRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    /// <summary>
    /// Gets the aura range for a given affinity level.
    /// </summary>
    private float GetAuraRangeForLevel(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                return baseAuraRange * 0.5f;
            case AffinityLevel.Stranger:
                return baseAuraRange;
            case AffinityLevel.Acquainted:
                return Mathf.Lerp(baseAuraRange, maxAuraRange, 0.25f);
            case AffinityLevel.Bonded:
                return Mathf.Lerp(baseAuraRange, maxAuraRange, 0.5f);
            case AffinityLevel.Devoted:
                return Mathf.Lerp(baseAuraRange, maxAuraRange, 0.75f);
            case AffinityLevel.Ascended:
                return maxAuraRange;
            default:
                return baseAuraRange;
        }
    }
    
    /// <summary>
    /// Gets the scale multiplier for a given affinity level.
    /// </summary>
    private float GetScaleMultiplierForLevel(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                return baseScale * 0.9f;
            case AffinityLevel.Stranger:
                return baseScale;
            case AffinityLevel.Acquainted:
                return baseScale + ascendedScaleBonus * 0.25f;
            case AffinityLevel.Bonded:
                return baseScale + ascendedScaleBonus * 0.5f;
            case AffinityLevel.Devoted:
                return baseScale + ascendedScaleBonus * 0.75f;
            case AffinityLevel.Ascended:
                return baseScale + ascendedScaleBonus;
            default:
                return baseScale;
        }
    }
    
    /// <summary>
    /// Gets the particle emission rate for a given affinity level.
    /// </summary>
    private float GetEmissionRateForLevel(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                return baseEmissionRate * 0.5f;
            case AffinityLevel.Stranger:
                return baseEmissionRate;
            case AffinityLevel.Acquainted:
                return Mathf.Lerp(baseEmissionRate, maxEmissionRate, 0.25f);
            case AffinityLevel.Bonded:
                return Mathf.Lerp(baseEmissionRate, maxEmissionRate, 0.5f);
            case AffinityLevel.Devoted:
                return Mathf.Lerp(baseEmissionRate, maxEmissionRate, 0.75f);
            case AffinityLevel.Ascended:
                return maxEmissionRate;
            default:
                return baseEmissionRate;
        }
    }
    
    /// <summary>
    /// Event handler for affinity level changes.
    /// </summary>
    private void OnAffinityLevelChanged(string entityId, AffinityLevel newLevel)
    {
        if (entity != null && entity.entityId == entityId)
        {
            Debug.Log($"[VISUAL] {entityId} visual effect responding to level change: {newLevel}");
            UpdateTargetValues(newLevel);
        }
    }
    
    /// <summary>
    /// Gets the color for a specific affinity level.
    /// </summary>
    public Color GetColorForLevel(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                return hostileColor;
            case AffinityLevel.Stranger:
                return strangerColor;
            case AffinityLevel.Acquainted:
                return acquaintedColor;
            case AffinityLevel.Bonded:
                return bondedColor;
            case AffinityLevel.Devoted:
                return devotedColor;
            case AffinityLevel.Ascended:
                return ascendedColor;
            default:
                return strangerColor;
        }
    }
    
    /// <summary>
    /// Manually triggers a flash effect (e.g., when ability is used).
    /// </summary>
    public void TriggerFlash(Color flashColor, float duration = 0.5f)
    {
        StartCoroutine(FlashEffect(flashColor, duration));
    }
    
    private IEnumerator FlashEffect(Color flashColor, float duration)
    {
        Color originalColor = currentColor;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Flash in then out
            float intensity = t < 0.5f ? t * 2f : (1f - t) * 2f;
            Color lerpColor = Color.Lerp(originalColor, flashColor, intensity);
            
            if (auraLight != null)
            {
                auraLight.color = lerpColor;
                auraLight.intensity = currentGlowIntensity * (1f + intensity);
            }
            
            yield return null;
        }
        
        // Restore original color
        if (auraLight != null)
        {
            auraLight.color = currentColor;
        }
    }
}
