using UnityEngine;

/// <summary>
/// Handles the visual representation of the tether between a player and their summoned Parent.
/// Uses a LineRenderer to draw a glowing line that changes color based on the Parent's temperament status.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TetherVisualEffect : MonoBehaviour
{
    [Header("Tether References")]
    public TetherSystem tetherSystem;
    public Transform playerTransform;
    public Transform summonTransform;
    
    [Header("Visual Settings")]
    public float baseWidth = 0.1f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.02f;
    public Material tetherMaterial;
    
    private static Material defaultTetherMaterial;
    
    [Header("Color States")]
    public Color normalColor = new Color(0.4f, 0.6f, 1f); // Blue - normal state
    public Color strainedColor = new Color(1f, 0.8f, 0.2f); // Yellow - health is low
    public Color criticalColor = new Color(1f, 0.2f, 0.2f); // Red - about to break
    public Color violatingColor = new Color(0.8f, 0.2f, 0.8f); // Purple - temperament violation
    
    [Header("Strain Thresholds")]
    [Range(0, 1)] public float strainedThreshold = 0.5f; // 50% health
    [Range(0, 1)] public float criticalThreshold = 0.25f; // 25% health
    
    private LineRenderer lineRenderer;
    private bool isActive = false;
    private float currentPulse = 0f;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }
    
    void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = baseWidth;
        lineRenderer.endWidth = baseWidth;
        
        if (tetherMaterial != null)
        {
            lineRenderer.material = tetherMaterial;
        }
        else
        {
            // Use cached default material to avoid memory allocation
            if (defaultTetherMaterial == null)
            {
                defaultTetherMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            lineRenderer.material = defaultTetherMaterial;
        }
        
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;
        lineRenderer.enabled = false;
    }
    
    void Update()
    {
        if (tetherSystem != null && tetherSystem.isTethered)
        {
            if (!isActive)
            {
                ActivateTether();
            }
            
            UpdateTetherPositions();
            UpdateTetherColor();
            UpdatePulseEffect();
        }
        else if (isActive)
        {
            DeactivateTether();
        }
    }
    
    void ActivateTether()
    {
        isActive = true;
        lineRenderer.enabled = true;
        
        // Get references from tether system
        if (tetherSystem.player != null)
        {
            playerTransform = tetherSystem.player.transform;
        }
        if (tetherSystem.activeSummon != null)
        {
            summonTransform = tetherSystem.activeSummon.transform;
            
            // Set initial color based on summon type
            UpdateTetherColor();
        }
        
        Debug.Log("[VFX] Tether visual effect activated.");
    }
    
    void DeactivateTether()
    {
        isActive = false;
        lineRenderer.enabled = false;
        playerTransform = null;
        summonTransform = null;
        
        Debug.Log("[VFX] Tether visual effect deactivated.");
    }
    
    void UpdateTetherPositions()
    {
        if (playerTransform != null && summonTransform != null)
        {
            // Offset positions slightly upward to be more visible
            Vector3 playerPos = playerTransform.position + Vector3.up * 1f;
            Vector3 summonPos = summonTransform.position + Vector3.up * 0.5f;
            
            lineRenderer.SetPosition(0, playerPos);
            lineRenderer.SetPosition(1, summonPos);
        }
    }
    
    void UpdateTetherColor()
    {
        if (tetherSystem == null || tetherSystem.player == null) return;
        
        PlayerController player = tetherSystem.player;
        float healthPercent = player.currentHealth / player.maxHealth;
        
        Color targetColor;
        
        // Determine color based on health percentage
        if (healthPercent <= criticalThreshold)
        {
            targetColor = criticalColor;
        }
        else if (healthPercent <= strainedThreshold)
        {
            // Lerp between strained and critical
            float t = (healthPercent - criticalThreshold) / (strainedThreshold - criticalThreshold);
            targetColor = Color.Lerp(criticalColor, strainedColor, t);
        }
        else
        {
            // Lerp between normal and strained
            float t = (healthPercent - strainedThreshold) / (1f - strainedThreshold);
            targetColor = Color.Lerp(strainedColor, normalColor, t);
        }
        
        // Apply color with smooth transition
        lineRenderer.startColor = Color.Lerp(lineRenderer.startColor, targetColor, Time.deltaTime * 3f);
        lineRenderer.endColor = Color.Lerp(lineRenderer.endColor, targetColor, Time.deltaTime * 3f);
    }
    
    void UpdatePulseEffect()
    {
        // Pulse the width for a breathing effect
        currentPulse += Time.deltaTime * pulseSpeed;
        float pulseValue = Mathf.Sin(currentPulse) * pulseAmount;
        
        lineRenderer.startWidth = baseWidth + pulseValue;
        lineRenderer.endWidth = baseWidth + pulseValue;
    }
    
    /// <summary>
    /// Sets the tether color to indicate a temperament violation.
    /// </summary>
    public void ShowViolation()
    {
        lineRenderer.startColor = violatingColor;
        lineRenderer.endColor = violatingColor;
    }
    
    /// <summary>
    /// Creates a flash effect on the tether.
    /// </summary>
    public void FlashTether(Color flashColor, float duration = 0.2f)
    {
        StartCoroutine(FlashCoroutine(flashColor, duration));
    }
    
    private System.Collections.IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        Color originalStart = lineRenderer.startColor;
        Color originalEnd = lineRenderer.endColor;
        
        lineRenderer.startColor = flashColor;
        lineRenderer.endColor = flashColor;
        
        yield return new WaitForSeconds(duration);
        
        lineRenderer.startColor = originalStart;
        lineRenderer.endColor = originalEnd;
    }
    
    /// <summary>
    /// Gets the current tether length.
    /// </summary>
    public float GetTetherLength()
    {
        if (playerTransform != null && summonTransform != null)
        {
            return Vector3.Distance(playerTransform.position, summonTransform.position);
        }
        return 0f;
    }
}
