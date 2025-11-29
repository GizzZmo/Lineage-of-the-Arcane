using UnityEngine;

/// <summary>
/// Handles the "Risk/Reward" health drain logic for the Tether system.
/// This system connects players to their summoned Parents.
/// </summary>
public class TetherSystem : MonoBehaviour
{
    [Header("Tether Configuration")]
    public MagicParent activeSummon;
    public PlayerController player;
    public bool isTethered = false;
    
    [Header("Tether Visual Settings")]
    public Color tetherColor = Color.blue;
    public float tetherWidth = 0.1f;

    void Update()
    {
        if (isTethered && activeSummon != null)
        {
            MaintainTether();
            activeSummon.CheckTemperament(); // Check if the Parent is angry
        }
    }

    /// <summary>
    /// Initiates a tether between the player and a summon.
    /// </summary>
    /// <param name="summon">The MagicParent to tether.</param>
    public void InitiateTether(MagicParent summon)
    {
        if (player == null)
        {
            Debug.LogError("Cannot initiate tether: Player is null!");
            return;
        }
        
        if (summon == null)
        {
            Debug.LogError("Cannot initiate tether: Summon is null!");
            return;
        }
        
        activeSummon = summon;
        isTethered = true;
        activeSummon.OnSummon(player);
        Debug.Log($"Tether initiated with {activeSummon.entityName}");
    }

    void MaintainTether()
    {
        // Drain player health to keep summon alive
        float cost = activeSummon.tetherCostPerSecond * Time.deltaTime;
        
        if (player.currentHealth > cost)
        {
            player.currentHealth -= cost;
            // Visual effect: Grey bar increases
        }
        else
        {
            BreakTether();
        }
    }

    void BreakTether()
    {
        isTethered = false;
        Debug.LogWarning("THE TETHER SNAPS! The Parent goes RAMPANT.");
        
        if (activeSummon != null)
        {
            activeSummon.OnTetherBroken();
        }
        // Logic to make the Summon attack the player goes here
    }
    
    /// <summary>
    /// Manually severs the tether connection.
    /// </summary>
    public void SeverTether()
    {
        if (isTethered)
        {
            BreakTether();
        }
    }
}
