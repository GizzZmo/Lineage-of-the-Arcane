using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the tether connection status, including the connected Parent's name,
/// tether strength, and temperament status.
/// </summary>
public class TetherDisplayUI : MonoBehaviour
{
    [Header("References")]
    public TetherSystem tetherSystem;
    public Image tetherIconImage;
    public Image tetherStrengthFill;
    public Text parentNameText;
    public Text temperamentStatusText;
    public Image temperamentIndicator;
    
    [Header("Tether State Colors")]
    public Color connectedColor = new Color(0.4f, 0.8f, 1f); // Cyan
    public Color disconnectedColor = new Color(0.4f, 0.4f, 0.4f); // Grey
    public Color warningColor = new Color(1f, 0.8f, 0.2f); // Yellow
    public Color dangerColor = new Color(1f, 0.2f, 0.2f); // Red
    
    [Header("Temperament Colors")]
    public Color pleasedColor = new Color(0.4f, 1f, 0.4f); // Green
    public Color neutralColor = new Color(0.8f, 0.8f, 0.8f); // Light grey
    public Color displeasedColor = new Color(1f, 0.5f, 0.2f); // Orange
    public Color angryColor = new Color(1f, 0.2f, 0.2f); // Red
    
    [Header("Animation")]
    public float pulseSpeed = 2f;
    public float smoothSpeed = 5f;
    
    [Header("Display Settings")]
    public bool showParentName = true;
    public bool showTemperamentStatus = true;
    
    private bool isTethered = false;
    private float tetherStrength = 0f;
    private float pulseTimer = 0f;
    private TemperamentStatus currentStatus = TemperamentStatus.Neutral;
    
    public enum TemperamentStatus
    {
        Pleased,
        Neutral,
        Displeased,
        Angry
    }
    
    void Start()
    {
        // Initialize as disconnected
        UpdateDisconnectedState();
    }
    
    void Update()
    {
        if (tetherSystem == null) return;
        
        bool wasTethered = isTethered;
        isTethered = tetherSystem.isTethered;
        
        // Handle state change
        if (isTethered && !wasTethered)
        {
            OnTetherConnected();
        }
        else if (!isTethered && wasTethered)
        {
            OnTetherDisconnected();
        }
        
        if (isTethered)
        {
            UpdateConnectedState();
        }
        else
        {
            UpdateDisconnectedState();
        }
    }
    
    void OnTetherConnected()
    {
        Debug.Log("[UI] Tether display connected.");
        
        if (tetherSystem.activeSummon != null && parentNameText != null)
        {
            parentNameText.text = tetherSystem.activeSummon.entityName;
        }
    }
    
    void OnTetherDisconnected()
    {
        Debug.Log("[UI] Tether display disconnected.");
    }
    
    void UpdateConnectedState()
    {
        // Update tether icon color
        if (tetherIconImage != null)
        {
            tetherIconImage.color = connectedColor;
        }
        
        // Calculate and update tether strength based on player health
        UpdateTetherStrength();
        
        // Update temperament display
        UpdateTemperamentDisplay();
        
        // Update parent name visibility
        if (parentNameText != null)
        {
            parentNameText.enabled = showParentName;
        }
        
        // Update temperament status visibility
        if (temperamentStatusText != null)
        {
            temperamentStatusText.enabled = showTemperamentStatus;
        }
    }
    
    void UpdateDisconnectedState()
    {
        if (tetherIconImage != null)
        {
            tetherIconImage.color = disconnectedColor;
        }
        
        if (tetherStrengthFill != null)
        {
            tetherStrengthFill.fillAmount = 0f;
        }
        
        if (parentNameText != null)
        {
            parentNameText.text = "No Tether";
            parentNameText.enabled = true;
        }
        
        if (temperamentStatusText != null)
        {
            temperamentStatusText.text = "";
            temperamentStatusText.enabled = false;
        }
        
        if (temperamentIndicator != null)
        {
            temperamentIndicator.color = neutralColor;
        }
    }
    
    void UpdateTetherStrength()
    {
        if (tetherSystem.player == null) return;
        
        // Guard against division by zero
        if (tetherSystem.player.maxHealth <= 0)
        {
            tetherStrength = 0f;
            return;
        }
        
        // Calculate tether strength based on player health percentage
        float healthPercent = tetherSystem.player.currentHealth / tetherSystem.player.maxHealth;
        tetherStrength = Mathf.Clamp01(healthPercent);
        
        // Update fill image
        if (tetherStrengthFill != null)
        {
            tetherStrengthFill.fillAmount = Mathf.Lerp(
                tetherStrengthFill.fillAmount, 
                tetherStrength, 
                Time.deltaTime * smoothSpeed
            );
            
            // Update color based on strength
            if (tetherStrength <= 0.25f)
            {
                tetherStrengthFill.color = dangerColor;
                AddPulseEffect();
            }
            else if (tetherStrength <= 0.5f)
            {
                tetherStrengthFill.color = warningColor;
            }
            else
            {
                tetherStrengthFill.color = connectedColor;
            }
        }
    }
    
    void AddPulseEffect()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
        
        if (tetherIconImage != null)
        {
            Color c = tetherIconImage.color;
            c.a = 0.5f + pulse * 0.5f;
            tetherIconImage.color = c;
        }
    }
    
    void UpdateTemperamentDisplay()
    {
        // Update temperament indicator color
        Color targetColor = GetTemperamentColor(currentStatus);
        
        if (temperamentIndicator != null)
        {
            temperamentIndicator.color = Color.Lerp(
                temperamentIndicator.color, 
                targetColor, 
                Time.deltaTime * smoothSpeed
            );
        }
        
        // Update temperament text
        if (temperamentStatusText != null && showTemperamentStatus)
        {
            temperamentStatusText.text = GetTemperamentText(currentStatus);
            temperamentStatusText.color = targetColor;
        }
    }
    
    Color GetTemperamentColor(TemperamentStatus status)
    {
        switch (status)
        {
            case TemperamentStatus.Pleased:
                return pleasedColor;
            case TemperamentStatus.Neutral:
                return neutralColor;
            case TemperamentStatus.Displeased:
                return displeasedColor;
            case TemperamentStatus.Angry:
                return angryColor;
            default:
                return neutralColor;
        }
    }
    
    string GetTemperamentText(TemperamentStatus status)
    {
        switch (status)
        {
            case TemperamentStatus.Pleased:
                return "Pleased";
            case TemperamentStatus.Neutral:
                return "Neutral";
            case TemperamentStatus.Displeased:
                return "Displeased";
            case TemperamentStatus.Angry:
                return "ANGRY!";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// Sets the current temperament status.
    /// </summary>
    public void SetTemperamentStatus(TemperamentStatus status)
    {
        currentStatus = status;
    }
    
    /// <summary>
    /// Sets the tether system reference.
    /// </summary>
    public void SetTetherSystem(TetherSystem tether)
    {
        tetherSystem = tether;
    }
    
    /// <summary>
    /// Returns the current tether strength.
    /// </summary>
    public float GetTetherStrength()
    {
        return tetherStrength;
    }
    
    /// <summary>
    /// Returns whether a tether is currently active.
    /// </summary>
    public bool IsTethered()
    {
        return isTethered;
    }
    
    /// <summary>
    /// Triggers a flash effect on the tether display.
    /// </summary>
    public void FlashTetherWarning()
    {
        StartCoroutine(WarningFlashCoroutine());
    }
    
    private System.Collections.IEnumerator WarningFlashCoroutine()
    {
        if (tetherIconImage != null)
        {
            Color originalColor = tetherIconImage.color;
            tetherIconImage.color = dangerColor;
            yield return new WaitForSeconds(0.15f);
            tetherIconImage.color = originalColor;
        }
    }
}
