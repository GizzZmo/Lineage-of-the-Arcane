using UnityEngine;

/// <summary>
/// Ignis Mater, The Combustion - The Fire Mother entity.
/// She demands aggression and punishes hesitation.
/// </summary>
public class IgnisMater : MagicParent
{
    [Header("Ignis Settings")]
    public float hesitationThreshold = 3.0f; // Seconds without attacking before punishment
    public float punishmentDamage = 5.0f;
    public Color ambientColor = Color.red;
    
    private void Start()
    {
        entityName = "Ignis Mater, The Combustion";
        tetherCostPerSecond = 10.0f; // High cost
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to turn the floor to lava or make lighting red
        RenderSettings.ambientLight = ambientColor;
        Debug.Log("The world heats up. Ignis is watching.");
    }

    public override void CheckTemperament()
    {
        if (boundPlayer == null) return;
        
        // Mechanic: If player hasn't attacked in threshold seconds, punish them
        if (Time.time - boundPlayer.lastAttackTime > hesitationThreshold)
        {
            PunishPlayer();
        }
    }

    void PunishPlayer()
    {
        if (boundPlayer != null)
        {
            boundPlayer.TakeDamage(punishmentDamage);
            Debug.Log("Ignis burns you for your hesitation!");
        }
    }
    
    public override void OnTetherBroken()
    {
        base.OnTetherBroken();
        Debug.LogWarning("Ignis Mater erupts in fury! Everything burns!");
        // Rampant behavior: Ignis attacks everything nearby
    }
}
