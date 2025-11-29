using UnityEngine;

/// <summary>
/// Handles player state, health, and combat tracking for the Tether system.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100.0f;
    public float currentHealth;
    
    [Header("Sanity Settings")]
    public float maxSanity = 100.0f;
    public float currentSanity;
    
    [Header("Combat Tracking")]
    public float lastAttackTime = 0f;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        currentSanity = maxSanity;
    }
    
    void Update()
    {
        HandleMovement();
        HandleCombatInput();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        
        // Use Rigidbody for physics-based movement when available
        if (rb != null)
        {
            rb.MovePosition(transform.position + movement);
        }
        else
        {
            transform.Translate(movement);
        }
    }
    
    void HandleCombatInput()
    {
        // Primary attack
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }
    
    public void PerformAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("Player attacks!");
        // Attack logic implementation would go here
    }
    
    /// <summary>
    /// Applies damage to the player.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Reduces player sanity.
    /// </summary>
    /// <param name="amount">Amount of sanity to drain.</param>
    public void DrainSanity(float amount)
    {
        currentSanity -= amount;
        Debug.Log($"Player sanity drained by {amount}. Current sanity: {currentSanity}");
        
        if (currentSanity <= 0)
        {
            OnSanityDepleted();
        }
    }
    
    void Die()
    {
        Debug.LogWarning("Player has died!");
        // Death logic implementation would go here
    }
    
    void OnSanityDepleted()
    {
        Debug.LogWarning("Player's sanity has been depleted! Madness takes hold.");
        // Sanity depletion effects would go here
    }
}
