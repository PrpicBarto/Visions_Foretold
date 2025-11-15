using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool initializeHealthOnStart = true;

    [Header("Death Settings")]
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private float ragdollLifetime = 10f;

    [Header("Health Regeneration")]
    [SerializeField] private bool enableHealthRegeneration = false;
    [SerializeField] private float healthRegenRate = 1f; // Health per second
    [SerializeField] private float healthRegenDelay = 3f; // Delay after taking damage
    [SerializeField] private int healthRegenThreshold = 50; // Only regen below this percentage

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public UnityEvent<int> OnDamageTaken; // (damageAmount)
    public UnityEvent<int> OnHealthRestored; // (healAmount)
    public UnityEvent OnDeath;
    public UnityEvent OnFullHealth;

    // Private variables for optimization
    private float lastDamageTime = -Mathf.Infinity;
    public bool isDead = false;
    private float healthRegenTimer = 0f;

    // Properties for external access
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsAtFullHealth => currentHealth >= maxHealth;

    #region Unity Lifecycle

    private void Awake()
    {
        // Initialize health if not set in inspector
        if (currentHealth <= 0 && initializeHealthOnStart)
        {
            currentHealth = maxHealth;
        }
    }

    private void Start()
    {
        // Invoke initial health state
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsAtFullHealth)
        {
            OnFullHealth?.Invoke();
        }
    }

    private void Update()
    {
        if (enableHealthRegeneration && !isDead)
        {
            HandleHealthRegeneration();
        }
    }

    #endregion

    #region Damage and Healing Methods

    /// <summary>
    /// Deals damage to this health component (compatible with PlayerAttack)
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void DealDamage(int damage)
    {
        TakeDamage(damage);
    }

    /// <summary>
    /// Takes damage - preferred method name (matches EnemyHealth pattern)
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;

        // Reset health regen timer when taking damage
        healthRegenTimer = 0f;

        // Invoke events
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the health component (compatible with PlayerAttack heal spell)
    /// </summary>
    /// <param name="healAmount">Amount of health to restore</param>
    public void Heal(int healAmount)
    {
        AddHealth(healAmount);
    }

    /// <summary>
    /// Adds health to the current health
    /// </summary>
    /// <param name="amount">Amount of health to add</param>
    public void AddHealth(int amount)
    {
        if (isDead || amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // Only invoke events if health actually changed
        if (currentHealth != previousHealth)
        {
            OnHealthRestored?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"{gameObject.name} healed {amount} health. Health: {currentHealth}/{maxHealth}");

            // Check if at full health
            if (IsAtFullHealth)
            {
                OnFullHealth?.Invoke();
            }
        }
    }

    /// <summary>
    /// Restores health to full
    /// </summary>
    public void RestoreToFullHealth()
    {
        int healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            AddHealth(healAmount);
        }
    }

    /// <summary>
    /// Sets health to a specific value
    /// </summary>
    /// <param name="newHealth">New health value</param>
    public void SetHealth(int newHealth)
    {
        newHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (newHealth != currentHealth)
        {
            int difference = newHealth - currentHealth;
            currentHealth = newHealth;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (difference > 0)
            {
                OnHealthRestored?.Invoke(difference);
            }
            else if (difference < 0)
            {
                OnDamageTaken?.Invoke(-difference);
            }

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
            else if (IsAtFullHealth)
            {
                OnFullHealth?.Invoke();
            }
        }
    }

    #endregion

    #region Health Regeneration

    private void HandleHealthRegeneration()
    {
        // Only regenerate if below threshold and enough time has passed since last damage
        if (HealthPercentage >= (healthRegenThreshold / 100f) ||
            Time.time < lastDamageTime + healthRegenDelay)
        {
            return;
        }

        healthRegenTimer += Time.deltaTime;

        if (healthRegenTimer >= 1f / healthRegenRate)
        {
            AddHealth(1);
            healthRegenTimer = 0f;
        }
    }

    #endregion

    #region Death System

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has died.");

        // Invoke death event
        OnDeath?.Invoke();

        // Handle ragdoll instantiation
        if (ragdollPrefab != null)
        {
            SpawnRagdoll();
        }

        // Handle player-specific death logic
        if (isPlayer)
        {
            HandlePlayerDeath();
        }
        else
        {
            // For non-player entities, destroy after a short delay
            Destroy(gameObject, 0.1f);
        }
    }

    private void SpawnRagdoll()
    {
        GameObject ragdollInstance = Instantiate(ragdollPrefab, transform.position, transform.rotation);

        // Copy velocity if this object has a rigidbody
        Rigidbody originalRb = GetComponent<Rigidbody>();
        if (originalRb != null)
        {
            Rigidbody[] ragdollRigidbodies = ragdollInstance.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in ragdollRigidbodies)
            {
                rb.linearVelocity = originalRb.linearVelocity;
                rb.angularVelocity = originalRb.angularVelocity;
            }
        }

        // Auto-destroy ragdoll after specified time
        if (ragdollLifetime > 0)
        {
            Destroy(ragdollInstance, ragdollLifetime);
        }
    }

    private void HandlePlayerDeath()
    {
        // Player-specific death logic
        // Disable player controls
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        // Disable movement
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable attack
        var playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        // TODO: Implement respawn system, game over screen, etc.
        Debug.Log("Player died! Implement respawn or game over logic here.");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Resets health component to initial state (useful for respawning)
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        lastDamageTime = -Mathf.Infinity;
        healthRegenTimer = 0f;

        // Re-enable components if they were disabled
        if (isPlayer)
        {
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null) playerInput.enabled = true;

            var playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;

            var playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack != null) playerAttack.enabled = true;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnFullHealth?.Invoke();
    }

    /// <summary>
    /// Sets the maximum health (useful for upgrades)
    /// </summary>
    /// <param name="newMaxHealth">New maximum health value</param>
    /// <param name="adjustCurrentHealth">Whether to adjust current health proportionally</param>
    public void SetMaxHealth(int newMaxHealth, bool adjustCurrentHealth = false)
    {
        if (newMaxHealth <= 0) return;

        if (adjustCurrentHealth)
        {
            float healthRatio = HealthPercentage;
            maxHealth = newMaxHealth;
            currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
        }
        else
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    #endregion

    #region Debug and Validation

    private void OnValidate()
    {
        // Ensure health values are valid in the inspector
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthRegenRate = Mathf.Max(0.1f, healthRegenRate);
        healthRegenDelay = Mathf.Max(0f, healthRegenDelay);
        healthRegenThreshold = Mathf.Clamp(healthRegenThreshold, 0, 100);
    }

    #endregion
}