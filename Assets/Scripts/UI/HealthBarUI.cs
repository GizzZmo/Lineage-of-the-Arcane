using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's health bar with a burned/grey overlay showing max health loss from tethering.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public Image healthFillImage;
    public Image burnedHealthImage;
    public Image backgroundImage;
    public Text healthText;
    
    [Header("Settings")]
    public Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f); // Green
    public Color midHealthColor = new Color(0.9f, 0.8f, 0.2f); // Yellow
    public Color lowHealthColor = new Color(0.8f, 0.2f, 0.2f); // Red
    public Color burnedHealthColor = new Color(0.3f, 0.3f, 0.3f); // Grey
    
    [Header("Thresholds")]
    [Range(0, 1)] public float lowHealthThreshold = 0.25f;
    [Range(0, 1)] public float midHealthThreshold = 0.5f;
    
    [Header("Animation")]
    public float smoothSpeed = 5f;
    public bool showNumbers = true;
    public bool animateDamage = true;
    
    private float targetFill = 1f;
    private float burnedHealthAmount = 0f;
    private float previousMaxHealth;
    
    void Start()
    {
        if (player != null)
        {
            previousMaxHealth = player.maxHealth;
        }
        
        if (burnedHealthImage != null)
        {
            burnedHealthImage.color = burnedHealthColor;
            burnedHealthImage.fillAmount = 0f;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        UpdateHealthBar();
        UpdateBurnedHealth();
        UpdateHealthText();
    }
    
    void UpdateHealthBar()
    {
        // Calculate target fill amount
        targetFill = player.currentHealth / player.maxHealth;
        targetFill = Mathf.Clamp01(targetFill);
        
        // Smoothly animate the health bar
        if (healthFillImage != null)
        {
            if (animateDamage)
            {
                healthFillImage.fillAmount = Mathf.Lerp(
                    healthFillImage.fillAmount, 
                    targetFill, 
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                healthFillImage.fillAmount = targetFill;
            }
            
            // Update color based on health percentage
            healthFillImage.color = GetHealthColor(targetFill);
        }
    }
    
    void UpdateBurnedHealth()
    {
        // Track max health reduction (burned health from tethering)
        if (player.maxHealth < previousMaxHealth)
        {
            // Calculate how much max health was burned
            float burnedPercent = 1f - (player.maxHealth / previousMaxHealth);
            burnedHealthAmount = Mathf.Max(burnedHealthAmount, burnedPercent);
        }
        
        if (burnedHealthImage != null)
        {
            burnedHealthImage.fillAmount = burnedHealthAmount;
        }
        
        previousMaxHealth = player.maxHealth;
    }
    
    void UpdateHealthText()
    {
        if (healthText != null && showNumbers)
        {
            healthText.text = $"{Mathf.CeilToInt(player.currentHealth)} / {Mathf.CeilToInt(player.maxHealth)}";
        }
    }
    
    Color GetHealthColor(float healthPercent)
    {
        if (healthPercent <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (healthPercent <= midHealthThreshold)
        {
            // Lerp between low and mid
            float t = (healthPercent - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else
        {
            // Lerp between mid and full
            float t = (healthPercent - midHealthThreshold) / (1f - midHealthThreshold);
            return Color.Lerp(midHealthColor, fullHealthColor, t);
        }
    }
    
    /// <summary>
    /// Sets the player reference for this health bar.
    /// </summary>
    public void SetPlayer(PlayerController newPlayer)
    {
        player = newPlayer;
        if (player != null)
        {
            previousMaxHealth = player.maxHealth;
        }
    }
    
    /// <summary>
    /// Resets the burned health display.
    /// </summary>
    public void ResetBurnedHealth()
    {
        burnedHealthAmount = 0f;
        if (burnedHealthImage != null)
        {
            burnedHealthImage.fillAmount = 0f;
        }
    }
    
    /// <summary>
    /// Triggers a damage flash effect on the health bar.
    /// </summary>
    public void FlashDamage()
    {
        StartCoroutine(DamageFlashCoroutine());
    }
    
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        if (backgroundImage != null)
        {
            Color originalColor = backgroundImage.color;
            backgroundImage.color = lowHealthColor;
            yield return new WaitForSeconds(0.1f);
            backgroundImage.color = originalColor;
        }
    }
}
