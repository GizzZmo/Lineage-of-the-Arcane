using UnityEngine;

/// <summary>
/// The Blueprint for all "Parents of Magic".
/// This abstract class defines what a "Parent" is and handles personality traits.
/// </summary>
public abstract class MagicParent : MonoBehaviour
{
    [Header("Entity Stats")]
    public string entityName;
    public float tetherCostPerSecond = 5.0f; // How much Health/Sanity it costs
    [Range(0, 100)] public float defianceLevel = 0f; // Chance to ignore orders

    protected PlayerController boundPlayer;

    /// <summary>
    /// Called when the player initiates the Tether.
    /// </summary>
    /// <param name="player">The player forming the tether.</param>
    public virtual void OnSummon(PlayerController player)
    {
        boundPlayer = player;
        Debug.Log($"{entityName} has entered the reality. The Tether is formed.");
        ApplyEnvironmentalShift();
    }

    /// <summary>
    /// Every Parent changes the game world physics/lighting.
    /// </summary>
    protected abstract void ApplyEnvironmentalShift();

    /// <summary>
    /// The unique passive rule (e.g., "Must keep attacking").
    /// Each Parent has different temperament requirements.
    /// </summary>
    public abstract void CheckTemperament();
    
    /// <summary>
    /// Called when the tether breaks.
    /// Override this to implement specific rampant behavior.
    /// </summary>
    public virtual void OnTetherBroken()
    {
        Debug.LogWarning($"{entityName} is now RAMPANT! The bond has been severed.");
    }
}
