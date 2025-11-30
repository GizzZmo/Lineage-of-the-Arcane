using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's sanity indicator with peripheral screen effects at low sanity.
/// </summary>
public class SanityIndicatorUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public Image sanityFillImage;
    public Image vignetteImage;
    public Image distortionOverlay;
    public Text sanityText;
    
    [Header("Sanity Colors")]
    public Color normalSanityColor = new Color(0.5f, 0.7f, 1f); // Light blue
    public Color midSanityColor = new Color(0.7f, 0.5f, 0.8f); // Purple
    public Color lowSanityColor = new Color(0.3f, 0.1f, 0.3f); // Dark purple
    public Color vignetteColor = new Color(0f, 0f, 0f, 0.8f); // Black vignette
    
    [Header("Thresholds")]
    [Range(0, 1)] public float lowSanityThreshold = 0.3f;
    [Range(0, 1)] public float midSanityThreshold = 0.6f;
    [Range(0, 1)] public float criticalSanityThreshold = 0.15f;
    
    [Header("Effect Settings")]
    public float vignetteMaxIntensity = 0.6f;
    public float pulseSpeed = 2f;
    public float distortionIntensity = 0.3f;
    public bool showNumbers = false;
    
    [Header("Animation")]
    public float smoothSpeed = 3f;
    public bool animateChanges = true;
    
    private float targetFill = 1f;
    private bool isLowSanity = false;
    private float pulseTimer = 0f;
    
    void Start()
    {
        // Initialize vignette as invisible
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
        
        // Initialize distortion as invisible
        if (distortionOverlay != null)
        {
            Color c = distortionOverlay.color;
            c.a = 0f;
            distortionOverlay.color = c;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        UpdateSanityBar();
        UpdateVignetteEffect();
        UpdateDistortionEffect();
        UpdateSanityText();
        
        // Update audio manager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSanityAudio(player.currentSanity);
        }
    }
    
    void UpdateSanityBar()
    {
        // Calculate target fill amount
        targetFill = player.currentSanity / player.maxSanity;
        targetFill = Mathf.Clamp01(targetFill);
        
        // Update low sanity state
        isLowSanity = targetFill <= lowSanityThreshold;
        
        // Smoothly animate the sanity bar
        if (sanityFillImage != null)
        {
            if (animateChanges)
            {
                sanityFillImage.fillAmount = Mathf.Lerp(
                    sanityFillImage.fillAmount, 
                    targetFill, 
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                sanityFillImage.fillAmount = targetFill;
            }
            
            // Update color based on sanity percentage
            sanityFillImage.color = GetSanityColor(targetFill);
        }
    }
    
    void UpdateVignetteEffect()
    {
        if (vignetteImage == null) return;
        
        float vignetteAlpha = 0f;
        
        if (targetFill <= lowSanityThreshold)
        {
            // Calculate vignette intensity based on how low sanity is
            float sanityInLowRange = targetFill / lowSanityThreshold;
            vignetteAlpha = (1f - sanityInLowRange) * vignetteMaxIntensity;
            
            // Add pulse effect at critical sanity
            if (targetFill <= criticalSanityThreshold)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f * 0.2f;
                vignetteAlpha += pulse;
            }
        }
        
        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, vignetteAlpha, Time.deltaTime * 2f);
        vignetteImage.color = c;
    }
    
    void UpdateDistortionEffect()
    {
        if (distortionOverlay == null) return;
        
        float distortionAlpha = 0f;
        
        if (targetFill <= criticalSanityThreshold)
        {
            // Apply distortion at critical sanity
            float criticalRatio = targetFill / criticalSanityThreshold;
            distortionAlpha = (1f - criticalRatio) * distortionIntensity;
        }
        
        Color c = distortionOverlay.color;
        c.a = Mathf.Lerp(c.a, distortionAlpha, Time.deltaTime * 2f);
        distortionOverlay.color = c;
    }
    
    void UpdateSanityText()
    {
        if (sanityText != null && showNumbers)
        {
            float sanityPercent = (player.currentSanity / player.maxSanity) * 100f;
            sanityText.text = $"Sanity: {Mathf.CeilToInt(sanityPercent)}%";
        }
    }
    
    Color GetSanityColor(float sanityPercent)
    {
        if (sanityPercent <= lowSanityThreshold)
        {
            return lowSanityColor;
        }
        else if (sanityPercent <= midSanityThreshold)
        {
            float t = (sanityPercent - lowSanityThreshold) / (midSanityThreshold - lowSanityThreshold);
            return Color.Lerp(lowSanityColor, midSanityColor, t);
        }
        else
        {
            float t = (sanityPercent - midSanityThreshold) / (1f - midSanityThreshold);
            return Color.Lerp(midSanityColor, normalSanityColor, t);
        }
    }
    
    /// <summary>
    /// Sets the player reference for this sanity indicator.
    /// </summary>
    public void SetPlayer(PlayerController newPlayer)
    {
        player = newPlayer;
    }
    
    /// <summary>
    /// Returns whether the player is currently in a low sanity state.
    /// </summary>
    public bool IsLowSanity()
    {
        return isLowSanity;
    }
    
    /// <summary>
    /// Returns whether the player is at critical sanity.
    /// </summary>
    public bool IsCriticalSanity()
    {
        return targetFill <= criticalSanityThreshold;
    }
    
    /// <summary>
    /// Triggers a sanity drain flash effect.
    /// </summary>
    public void FlashSanityDrain()
    {
        StartCoroutine(SanityFlashCoroutine());
    }
    
    private System.Collections.IEnumerator SanityFlashCoroutine()
    {
        if (sanityFillImage != null)
        {
            Color originalColor = sanityFillImage.color;
            sanityFillImage.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sanityFillImage.color = originalColor;
        }
    }
}
