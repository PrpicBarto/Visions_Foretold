using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackMode
    {
        Melee,
        Ranged,
        SpellWielding
    }

    public enum SpellType
    {
        Fireball,
        Lightning,
        IceBlast,
        Heal
    }

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private AttackMode currentAttackMode = AttackMode.Melee;
    [SerializeField] private TMP_Text attackModeText;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowProjectilePrefab;
    [SerializeField] private GameObject fireballProjectilePrefab;
    [SerializeField] private GameObject iceBlastProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Aiming Settings")]
    [SerializeField] private bool useAimTarget = true; // Whether to use aim target or player forward
    [SerializeField] private float aimHeightOffset = 1.0f; // Height adjustment for projectile spawn
    [SerializeField] private LayerMask aimingLayerMask = -1; // What layers to consider for aiming

    [Header("Spell Settings")]
    [SerializeField] private SpellType currentSpell = SpellType.Fireball;
    [SerializeField] private TMP_Text currentSpellText;
    [SerializeField] private float spellCastRange = 15.0f;
    [SerializeField] private Transform spellCastPoint; // Where spells are cast from

    [Header("Spell Cooldowns")]
    [SerializeField] private float fireballCooldown = 2.0f;
    [SerializeField] private float lightningCooldown = 3.0f;
    [SerializeField] private float iceBlastCooldown = 2.5f;
    [SerializeField] private float healCooldown = 5.0f;

    [Header("Mode Switch Settings")]
    [SerializeField] private float modeSwitchCooldown = 0.2f; // Reduced for scroll wheel responsiveness
    [SerializeField] private float spellSwitchCooldown = 0.3f;
    [SerializeField] private float scrollSensitivity = 0.1f; // Minimum scroll delta to register

    private float lastModeSwitchTime = -Mathf.Infinity;
    private float lastSpellSwitchTime = -Mathf.Infinity;

    private float lastAttackTime = -Mathf.Infinity;
    private float lastFireballTime = -Mathf.Infinity;
    private float lastLightningTime = -Mathf.Infinity;
    private float lastIceBlastTime = -Mathf.Infinity;
    private float lastHealTime = -Mathf.Infinity;

    // Input System
    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction scrollWheelAction;
    private InputAction nextSpellAction;
    private InputAction previousSpellAction;
    private InputAction gamePadModeSwitchAction; // Gamepad alternative for mode switching

    // Aim target reference (from PlayerMovement)
    private PlayerMovement playerMovement;
    private Transform aimTarget;

    // Camera reference for better aiming
    private Camera mainCamera;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        mainCamera = Camera.main;

        // Find input actions
        attackAction = playerInput.actions.FindAction("Attack");
        scrollWheelAction = playerInput.actions.FindAction("ScrollWheel");
        nextSpellAction = playerInput.actions.FindAction("NextSpell");
        previousSpellAction = playerInput.actions.FindAction("PreviousSpell");
        gamePadModeSwitchAction = playerInput.actions.FindAction("GamepadModeSwitch");

        // Set default projectile spawn point if not assigned
        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }

        // Set default spell cast point if not assigned
        if (spellCastPoint == null)
        {
            spellCastPoint = transform;
        }

        // Get aim target directly from PlayerMovement
        GetAimTargetFromPlayerMovement();
    }

    /// <summary>
    /// Get the aim target from PlayerMovement component
    /// </summary>
    private void GetAimTargetFromPlayerMovement()
    {
        if (playerMovement != null)
        {
            aimTarget = playerMovement.AimTarget;
            if (aimTarget != null)
            {
                Debug.Log($"Successfully found aim target: {aimTarget.name}");
            }
            else
            {
                Debug.LogWarning("PlayerMovement.AimTarget is null! Make sure it's assigned in the inspector.");
            }
        }
        else
        {
            Debug.LogError("PlayerMovement component not found!");
        }
    }

    private void OnEnable()
    {
        // Subscribe to input events
        if (scrollWheelAction != null) scrollWheelAction.performed += OnScrollWheel;
        if (nextSpellAction != null) nextSpellAction.performed += OnNextSpell;
        if (previousSpellAction != null) previousSpellAction.performed += OnPreviousSpell;
        if (gamePadModeSwitchAction != null) gamePadModeSwitchAction.performed += OnGamepadModeSwitch;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        if (scrollWheelAction != null) scrollWheelAction.performed -= OnScrollWheel;
        if (nextSpellAction != null) nextSpellAction.performed -= OnNextSpell;
        if (previousSpellAction != null) previousSpellAction.performed -= OnPreviousSpell;
        if (gamePadModeSwitchAction != null) gamePadModeSwitchAction.performed -= OnGamepadModeSwitch;
    }

    private void Start()
    {
        UpdateAttackModeText();
        UpdateSpellText();
    }

    #region Input Handlers

    public void PerformAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || Time.time < lastAttackTime + attackCooldown)
            return;

        switch (currentAttackMode)
        {
            case AttackMode.Melee:
                PerformMeleeAttack();
                break;
            case AttackMode.Ranged:
                PerformRangedAttack();
                break;
            case AttackMode.SpellWielding:
                CastSpell();
                break;
        }
        lastAttackTime = Time.time;
    }

    private void OnScrollWheel(InputAction.CallbackContext context)
    {
        if (Time.time < lastModeSwitchTime + modeSwitchCooldown)
            return;

        float scrollDelta = context.ReadValue<Vector2>().y;

        if (Mathf.Abs(scrollDelta) > scrollSensitivity)
        {
            int direction = scrollDelta > 0 ? 1 : -1;
            CycleAttackMode(direction);
            lastModeSwitchTime = Time.time;
            UpdateAttackModeText();
        }
    }

    private void OnGamepadModeSwitch(InputAction.CallbackContext context)
    {
        if (Time.time < lastModeSwitchTime + modeSwitchCooldown)
            return;

        CycleAttackMode(1); // Cycle forward on gamepad
        lastModeSwitchTime = Time.time;
        UpdateAttackModeText();
    }

    private void OnNextSpell(InputAction.CallbackContext context)
    {
        if (currentAttackMode != AttackMode.SpellWielding ||
            Time.time < lastSpellSwitchTime + spellSwitchCooldown)
            return;

        CycleSpell(1);
        lastSpellSwitchTime = Time.time;
        UpdateSpellText();
    }

    private void OnPreviousSpell(InputAction.CallbackContext context)
    {
        if (currentAttackMode != AttackMode.SpellWielding ||
            Time.time < lastSpellSwitchTime + spellSwitchCooldown)
            return;

        CycleSpell(-1);
        lastSpellSwitchTime = Time.time;
        UpdateSpellText();
    }

    #endregion

    #region Attack Methods

    private void PerformMeleeAttack()
    {
        // Use the same aiming logic for consistency
        Vector3 attackDirection = GetShootDirection();
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(attackOrigin, attackDirection, out RaycastHit hit, attackRange))
        {
            // Try new Health system first
            var targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
            }
            else
            {
                // Fallback to old system
                var enemyHealth = hit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
        Debug.Log("Player performed a melee attack.");
    }

    private void PerformRangedAttack()
    {
        if (arrowProjectilePrefab == null)
        {
            Debug.LogWarning("Arrow projectile prefab not assigned!");
            return;
        }

        Vector3 shootDirection = GetShootDirection();
        FireProjectile(arrowProjectilePrefab, shootDirection, projectileSpeed, ProjectileDamage.ProjectileType.Arrow);
        Debug.Log("Player shot an arrow!");
    }

    private void CastSpell()
    {
        if (!IsSpellReady(currentSpell))
        {
            Debug.Log($"{currentSpell} is still on cooldown!");
            return;
        }

        switch (currentSpell)
        {
            case SpellType.Fireball:
                CastFireball();
                lastFireballTime = Time.time;
                break;
            case SpellType.Lightning:
                CastLightning();
                lastLightningTime = Time.time;
                break;
            case SpellType.IceBlast:
                CastIceBlast();
                lastIceBlastTime = Time.time;
                break;
            case SpellType.Heal:
                CastHeal();
                lastHealTime = Time.time;
                break;
        }

        Debug.Log($"Player cast {currentSpell}!");
    }

    #endregion

    #region Projectile System

    private Vector3 GetShootDirection()
    {
        if (useAimTarget && aimTarget != null)
        {
            // Calculate direction to aim target with height adjustment
            Vector3 spawnPosition = GetProjectileSpawnPosition();
            Vector3 targetPosition = aimTarget.position;

            // Adjust target position to be at a reasonable height
            targetPosition.y = spawnPosition.y;

            Vector3 direction = (targetPosition - spawnPosition).normalized;

            Debug.DrawRay(spawnPosition, direction * 10f, Color.red, 0.5f);
            return direction;
        }
        else
        {
            // Use player's forward direction as fallback
            Debug.DrawRay(GetProjectileSpawnPosition(), transform.forward * 10f, Color.blue, 0.5f);
            return transform.forward;
        }
    }

    private Vector3 GetProjectileSpawnPosition()
    {
        Vector3 spawnPos = projectileSpawnPoint.position;
        spawnPos.y += aimHeightOffset;
        return spawnPos;
    }

    private void FireProjectile(GameObject projectilePrefab, Vector3 direction, float speed, ProjectileDamage.ProjectileType projectileType = ProjectileDamage.ProjectileType.Arrow)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = GetProjectileSpawnPosition();
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));

        // Initialize projectile damage component
        ProjectileDamage projectileDamage = projectile.GetComponent<ProjectileDamage>();
        if (projectileDamage != null)
        {
            projectileDamage.Initialize(attackDamage, gameObject);
            projectileDamage.SetProjectileType(projectileType);
        }

        // Add velocity to projectile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Destroy projectile after lifetime
        Destroy(projectile, projectileLifetime);
    }

    #endregion

    #region Spell Casting

    private bool IsSpellReady(SpellType spell)
    {
        return spell switch
        {
            SpellType.Fireball => Time.time >= lastFireballTime + fireballCooldown,
            SpellType.Lightning => Time.time >= lastLightningTime + lightningCooldown,
            SpellType.IceBlast => Time.time >= lastIceBlastTime + iceBlastCooldown,
            SpellType.Heal => Time.time >= lastHealTime + healCooldown,
            _ => true
        };
    }

    private void CastFireball()
    {
        if (fireballProjectilePrefab != null)
        {
            Vector3 castDirection = GetShootDirection();
            FireProjectile(fireballProjectilePrefab, castDirection, projectileSpeed * 0.8f, ProjectileDamage.ProjectileType.Fireball);
        }
        else
        {
            // Fallback to raycast-based fireball
            Vector3 castDirection = GetShootDirection();
            Vector3 castOrigin = GetProjectileSpawnPosition();

            if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, spellCastRange, aimingLayerMask))
            {
                // Apply area damage at hit point
                DealAreaDamage(hit.point, 3f, attackDamage * 2);
            }
            else
            {
                // Cast at maximum range
                Vector3 targetPoint = castOrigin + castDirection * spellCastRange;
                DealAreaDamage(targetPoint, 3f, attackDamage * 2);
            }
        }
        Debug.Log("Casting Fireball - dealing fire damage!");
    }

    private void CastLightning()
    {
        Vector3 castDirection = GetShootDirection();
        Vector3 castOrigin = GetProjectileSpawnPosition();

        if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, spellCastRange, aimingLayerMask))
        {
            // Try new Health system first
            var targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage * 3);
            }
            else
            {
                // Fallback to old system
                var enemyHealth = hit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage * 3);
                }
            }

            // TODO: Add lightning visual effect from cast point to hit point
            Debug.DrawLine(castOrigin, hit.point, Color.cyan, 1f);
        }
        Debug.Log("Casting Lightning - instant electrical damage!");
    }

    private void CastIceBlast()
    {
        if (iceBlastProjectilePrefab != null)
        {
            Vector3 castDirection = GetShootDirection();
            FireProjectile(iceBlastProjectilePrefab, castDirection, projectileSpeed * 0.6f, ProjectileDamage.ProjectileType.IceBlast);
        }
        else
        {
            // Fallback to area effect at target location
            Vector3 castDirection = GetShootDirection();
            Vector3 castOrigin = GetProjectileSpawnPosition();
            Vector3 targetPosition = castOrigin + castDirection * spellCastRange;

            DealAreaDamage(targetPosition, 4f, attackDamage);
        }
        Debug.Log("Casting Ice Blast - freezing area damage!");
    }

    private void CastHeal()
    {
        // Heal the player
        var playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.Heal(attackDamage * 2);
        }

        // TODO: Add healing visual effects
        Debug.Log("Casting Heal - restoring health!");
    }

    private void DealAreaDamage(Vector3 center, float radius, int damage)
    {
        Collider[] enemies = Physics.OverlapSphere(center, radius, aimingLayerMask);
        foreach (var enemy in enemies)
        {
            // Skip if it's the player
            if (enemy.gameObject == gameObject) continue;

            // Try new Health system first
            var targetHealth = enemy.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
            else
            {
                // Fallback to old system
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
    }

    #endregion

    #region Mode and Spell Cycling

    private void CycleAttackMode(int direction)
    {
        var modes = System.Enum.GetValues(typeof(AttackMode));
        int modeCount = modes.Length;
        int currentIndex = (int)currentAttackMode;
        int newIndex = (currentIndex + direction + modeCount) % modeCount;
        currentAttackMode = (AttackMode)modes.GetValue(newIndex);

        Debug.Log($"Switched to {currentAttackMode} mode");
    }

    private void CycleSpell(int direction)
    {
        var spells = System.Enum.GetValues(typeof(SpellType));
        int spellCount = spells.Length;
        int currentIndex = (int)currentSpell;
        int newIndex = (currentIndex + direction + spellCount) % spellCount;
        currentSpell = (SpellType)spells.GetValue(newIndex);

        Debug.Log($"Selected spell: {currentSpell}");
    }

    #endregion

    #region UI Updates

    private void UpdateAttackModeText()
    {
        if (attackModeText != null)
        {
            attackModeText.text = $"Mode: {currentAttackMode}";
        }
    }

    private void UpdateSpellText()
    {
        if (currentSpellText != null)
        {
            if (currentAttackMode == AttackMode.SpellWielding)
            {
                float cooldownRemaining = GetSpellCooldownRemaining(currentSpell);
                string cooldownText = cooldownRemaining > 0 ? $" ({cooldownRemaining:F1}s)" : " (Ready)";
                currentSpellText.text = $"Spell: {currentSpell}{cooldownText}";
            }
            else
            {
                currentSpellText.text = "";
            }
        }
    }

    private float GetSpellCooldownRemaining(SpellType spell)
    {
        return spell switch
        {
            SpellType.Fireball => Mathf.Max(0, fireballCooldown - (Time.time - lastFireballTime)),
            SpellType.Lightning => Mathf.Max(0, lightningCooldown - (Time.time - lastLightningTime)),
            SpellType.IceBlast => Mathf.Max(0, iceBlastCooldown - (Time.time - lastIceBlastTime)),
            SpellType.Heal => Mathf.Max(0, healCooldown - (Time.time - lastHealTime)),
            _ => 0
        };
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw attack range for melee
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw shooting direction
        if (Application.isPlaying)
        {
            Vector3 shootDir = GetShootDirection();
            Vector3 spawnPos = GetProjectileSpawnPosition();

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(spawnPos, shootDir * 10f);

            // Draw aim target if available
            if (aimTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
                Gizmos.DrawLine(spawnPos, aimTarget.position);
            }
        }
    }

    #endregion

    private void Update()
    {
        // Update spell text to show cooldown timers
        if (currentAttackMode == AttackMode.SpellWielding)
        {
            UpdateSpellText();
        }

        // Try to get aim target if we don't have it
        if (aimTarget == null && playerMovement != null)
        {
            GetAimTargetFromPlayerMovement();
        }
    }
}