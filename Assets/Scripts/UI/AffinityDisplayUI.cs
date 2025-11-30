using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying affinity information during gameplay.
/// Shows current affinity level, progress to next level, and entity relationship status.
/// </summary>
public class AffinityDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component for displaying entity name")]
    public Text entityNameText;
    
    [Tooltip("Text component for displaying affinity level")]
    public Text affinityLevelText;
    
    [Tooltip("Progress bar fill image for affinity progress")]
    public Image affinityProgressFill;
    
    [Tooltip("Text component for displaying affinity percentage")]
    public Text affinityPercentageText;
    
    [Tooltip("Icon to show when special ability is unlocked")]
    public GameObject specialAbilityIcon;
    
    [Tooltip("Text for displaying tether statistics")]
    public Text statisticsText;
    
    [Header("Level Colors")]
    public Color hostileColor = new Color(0.5f, 0f, 0f);
    public Color strangerColor = Color.gray;
    public Color acquaintedColor = new Color(0.8f, 0.8f, 0.4f);
    public Color bondedColor = new Color(0.4f, 0.8f, 0.4f);
    public Color devotedColor = new Color(0.4f, 0.6f, 1f);
    public Color ascendedColor = new Color(1f, 0.8f, 0f);
    
    [Header("Animation")]
    public float updateSpeed = 2f;
    public float glowPulseSpeed = 1.5f;
    
    [Header("Configuration")]
    [Tooltip("Reference to the TetherSystem for entity tracking")]
    public TetherSystem tetherSystem;
    
    private string currentEntityId;
    private float displayedProgress;
    private float glowIntensity;
    
    void Start()
    {
        // Subscribe to affinity events
        // The Instance property auto-creates if null, so we subscribe directly
        AffinitySystem.Instance.OnAffinityLevelChanged += HandleAffinityLevelChanged;
        AffinitySystem.Instance.OnAffinityGained += HandleAffinityGained;
        AffinitySystem.Instance.OnAbilityUnlocked += HandleAbilityUnlocked;
        
        // Initialize UI state
        if (specialAbilityIcon != null)
        {
            specialAbilityIcon.SetActive(false);
        }
        
        ClearDisplay();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events if the AffinitySystem still exists
        // Note: During scene unload, the singleton may already be destroyed
        AffinitySystem instance = FindObjectOfType<AffinitySystem>();
        if (instance != null)
        {
            instance.OnAffinityLevelChanged -= HandleAffinityLevelChanged;
            instance.OnAffinityGained -= HandleAffinityGained;
            instance.OnAbilityUnlocked -= HandleAbilityUnlocked;
        }
    }
    
    void Update()
    {
        if (tetherSystem != null && tetherSystem.isTethered && tetherSystem.activeSummon != null)
        {
            UpdateDisplay(tetherSystem.activeSummon);
        }
        
        // Animate glow effect for ascended level
        if (currentEntityId != null)
        {
            AffinityLevel level = AffinitySystem.Instance.GetAffinityLevel(currentEntityId);
            if (level == AffinityLevel.Ascended)
            {
                AnimateGlow();
            }
        }
    }
    
    /// <summary>
    /// Updates the display with the current entity's affinity information.
    /// </summary>
    /// <param name="entity">The entity to display.</param>
    public void UpdateDisplay(MagicParent entity)
    {
        if (entity == null) return;
        
        currentEntityId = entity.entityId;
        
        AffinityData data = AffinitySystem.Instance.GetAffinityData(currentEntityId);
        
        // Update entity name
        if (entityNameText != null)
        {
            entityNameText.text = entity.entityName;
        }
        
        // Update affinity level text and color
        if (affinityLevelText != null)
        {
            affinityLevelText.text = GetLevelDisplayName(data.level);
            affinityLevelText.color = GetLevelColor(data.level);
        }
        
        // Update progress bar
        float targetProgress = AffinitySystem.Instance.GetProgressToNextLevel(currentEntityId);
        displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * updateSpeed);
        
        if (affinityProgressFill != null)
        {
            affinityProgressFill.fillAmount = displayedProgress;
            affinityProgressFill.color = GetLevelColor(data.level);
        }
        
        // Update percentage text
        if (affinityPercentageText != null)
        {
            affinityPercentageText.text = $"{data.currentAffinity:F1}%";
        }
        
        // Update special ability icon
        if (specialAbilityIcon != null)
        {
            specialAbilityIcon.SetActive(data.hasUnlockedAbility);
        }
        
        // Update statistics
        if (statisticsText != null)
        {
            statisticsText.text = $"Tethers: {data.successfulTethers} | Time: {data.totalTimeTethered:F0}s";
        }
    }
    
    /// <summary>
    /// Clears the display when no entity is tethered.
    /// </summary>
    public void ClearDisplay()
    {
        currentEntityId = null;
        displayedProgress = 0f;
        
        if (entityNameText != null)
        {
            entityNameText.text = "No Entity";
        }
        
        if (affinityLevelText != null)
        {
            affinityLevelText.text = "-";
            affinityLevelText.color = strangerColor;
        }
        
        if (affinityProgressFill != null)
        {
            affinityProgressFill.fillAmount = 0f;
        }
        
        if (affinityPercentageText != null)
        {
            affinityPercentageText.text = "0%";
        }
        
        if (specialAbilityIcon != null)
        {
            specialAbilityIcon.SetActive(false);
        }
        
        if (statisticsText != null)
        {
            statisticsText.text = "";
        }
    }
    
    /// <summary>
    /// Gets the display name for an affinity level.
    /// </summary>
    private string GetLevelDisplayName(AffinityLevel level)
    {
        switch (level)
        {
            case AffinityLevel.Hostile:
                return "HOSTILE";
            case AffinityLevel.Stranger:
                return "Stranger";
            case AffinityLevel.Acquainted:
                return "Acquainted";
            case AffinityLevel.Bonded:
                return "Bonded";
            case AffinityLevel.Devoted:
                return "Devoted";
            case AffinityLevel.Ascended:
                return "ASCENDED";
            default:
                return "Unknown";
        }
    }
    
    /// <summary>
    /// Gets the color associated with an affinity level.
    /// </summary>
    private Color GetLevelColor(AffinityLevel level)
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
    /// Animates the glow effect for ascended entities.
    /// </summary>
    private void AnimateGlow()
    {
        glowIntensity = (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) / 2f;
        
        if (affinityProgressFill != null)
        {
            Color baseColor = ascendedColor;
            affinityProgressFill.color = Color.Lerp(baseColor, Color.white, glowIntensity * 0.3f);
        }
    }
    
    // Event handlers
    private void HandleAffinityLevelChanged(string entityId, AffinityLevel newLevel)
    {
        if (entityId == currentEntityId)
        {
            Debug.Log($"[UI] Affinity level changed: {GetLevelDisplayName(newLevel)}");
            // Could trigger animation or sound effect here
        }
    }
    
    private void HandleAffinityGained(string entityId, float amount)
    {
        if (entityId == currentEntityId)
        {
            // Could show floating text or particle effect
        }
    }
    
    private void HandleAbilityUnlocked(string entityId)
    {
        if (entityId == currentEntityId && specialAbilityIcon != null)
        {
            specialAbilityIcon.SetActive(true);
            Debug.Log($"[UI] Special ability unlocked for {entityId}!");
            // Could trigger unlock animation
        }
    }
}
