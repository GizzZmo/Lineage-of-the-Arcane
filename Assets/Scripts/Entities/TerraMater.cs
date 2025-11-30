using UnityEngine;

/// <summary>
/// Terra Mater, The Foundation - The Earth Mother entity.
/// She demands rhythmic actions and punishes inconsistency.
/// Her presence increases gravity and causes structures to emerge.
/// </summary>
public class TerraMater : MagicParent
{
    [Header("Terra Settings")]
    public float rhythmWindow = 3.0f; // Expected interval between actions
    public float rhythmTolerance = 0.5f; // Acceptable deviation from rhythm
    public float rhythmTimeoutMultiplier = 2.0f; // Multiplier for timeout grace period
    public float punishmentDamage = 6.0f;
    public Color ambientColor = new Color(0.6f, 0.4f, 0.2f); // Earthy brown
    public float gravityMultiplier = 1.5f; // Increases gravity by 50%
    
    private float lastActionTime;
    private float originalGravity;
    private int rhythmStreak = 0; // Tracks consistent rhythmic actions
    private bool hasStartedRhythm = false;
    
    private void Start()
    {
        entityName = "Terra Mater, The Foundation";
        tetherCostPerSecond = 7.0f; // Moderate cost
        
        // Configure rampant behavior specific to Terra
        rampantBehavior = RampantBehavior.Destructive; // Earth destroys environment
        rampantDuration = 40f;
        rampantDamage = 18f;
        
        ConfigureRampantState();
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to increase gravity and change lighting to earthy tones
        RenderSettings.ambientLight = ambientColor;
        originalGravity = Physics.gravity.y;
        Physics.gravity = new Vector3(0, originalGravity * gravityMultiplier, 0);
        Debug.Log("The earth trembles. Terra Mater's weight presses upon the world.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: Player must perform actions at a consistent rhythm
        // Check if the player has attacked and if it was rhythmic
        if (boundPlayer.lastAttackTime > lastActionTime)
        {
            float interval = boundPlayer.lastAttackTime - lastActionTime;
            
            if (hasStartedRhythm)
            {
                // Check if the interval is within acceptable rhythm tolerance
                float deviation = Mathf.Abs(interval - rhythmWindow);
                
                if (deviation <= rhythmTolerance)
                {
                    // Good rhythm!
                    rhythmStreak++;
                    Debug.Log($"Terra Mater approves of your rhythm. Streak: {rhythmStreak}");
                }
                else
                {
                    // Rhythm broken!
                    PunishPlayer();
                    rhythmStreak = 0;
                }
            }
            else
            {
                // First action establishes the rhythm
                hasStartedRhythm = true;
                Debug.Log("Terra Mater begins to measure your rhythm...");
            }
            
            lastActionTime = boundPlayer.lastAttackTime;
        }
        
        // Also punish if too much time passes without any action
        if (hasStartedRhythm && Time.time - lastActionTime > rhythmWindow + rhythmTolerance * rhythmTimeoutMultiplier)
        {
            PunishPlayer();
            hasStartedRhythm = false;
            rhythmStreak = 0;
            Debug.Log("Terra Mater is displeased by your silence.");
        }
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Terra Mater shakes the ground beneath you!");
        }
    }
    
    public override void OnTetherBroken()
    {
        // Restore gravity before going rampant
        if (originalGravity != 0)
        {
            Physics.gravity = new Vector3(0, originalGravity, 0);
        }
        
        base.OnTetherBroken();
        Debug.LogWarning("Terra Mater erupts in fury! The ground cracks and splits!");
        
        // Intensify environmental effects during rampant state
        RenderSettings.ambientLight = ambientColor * 1.5f;
        
        // Extreme gravity during rampant
        Physics.gravity = new Vector3(0, originalGravity * 2f, 0);
    }
    
    /// <summary>
    /// Called when Terra Mater is dismissed properly.
    /// </summary>
    public void OnDismiss()
    {
        if (originalGravity != 0)
        {
            Physics.gravity = new Vector3(0, originalGravity, 0);
        }
        RenderSettings.ambientLight = Color.white * 0.5f;
        Debug.Log("The earth settles. Terra Mater returns to her slumber.");
    }
    
    /// <summary>
    /// Gets the current rhythm streak count.
    /// </summary>
    public int GetRhythmStreak()
    {
        return rhythmStreak;
    }
    
    /// <summary>
    /// Bonus effect when rhythm is maintained well.
    /// </summary>
    public void ApplyRhythmBonus()
    {
        if (rhythmStreak >= 5 && boundPlayer != null)
        {
            // Reduce tether cost for good rhythm
            float reduction = tetherCostPerSecond * 0.1f * (rhythmStreak / 5);
            Debug.Log($"Terra Mater rewards your rhythm with reduced tether cost: {reduction}");
        }
    }
}
