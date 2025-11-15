using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ProjectileDamage : MonoBehaviour
{
    public enum ProjectileType
    {
        Arrow,
        Fireball,
        IceBlast,
        Lightning,
        EnemyProjectile
    }

    public enum DamageMode
    {
        SingleTarget,   // Damages only the first target hit
        AreaOfEffect,   // Damages all targets in an area
        Piercing        // Goes through multiple targets
    }

    [Header("Projectile Settings")]
    [SerializeField] private ProjectileType projectileType = ProjectileType.Arrow;
    [SerializeField] private DamageMode damageMode = DamageMode.SingleTarget;
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float damageFalloffDistance = 0f; // Distance at which damage starts to fall off
    [SerializeField] private float minDamageMultiplier = 0.5f; // Minimum damage as percentage of base

    [Header("Area of Effect Settings")]
    [SerializeField] private float aoeRadius = 3f;
    [SerializeField] private bool showAoeGizmo = true;
    [SerializeField] private LayerMask damageLayerMask = -1; // What layers can be damaged

    [Header("Piercing Settings")]
    [SerializeField] private int maxPierceTargets = 3;
    [SerializeField] private float damageLossPerPierce = 0.1f; // Damage reduction per pierce (0.1 = 10% loss)

    [Header("Special Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip hitSoundEffect;
    [SerializeField] private float effectLifetime = 2f;

    [Header("Status Effects")]
    [SerializeField] private bool canApplyStatusEffect = false;
    [SerializeField] private StatusEffectType statusEffectType = StatusEffectType.None;
    [SerializeField] private float statusEffectDuration = 3f;
    [SerializeField] private float statusEffectChance = 1f; // 0-1, chance to apply effect

    [Header("Knockback")]
    [SerializeField] private bool canKnockback = false;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackRadius = 2f;

    [Header("Events")]
    public UnityEvent<GameObject> OnProjectileHit; // Target hit
    public UnityEvent<Vector3> OnProjectileDestroyed; // Position where destroyed

    // Private variables
    private bool hasHit = false;
    private int pierceCount = 0;
    private float travelDistance = 0f;
    private Vector3 lastPosition;
    private Rigidbody projectileRigidbody;
    private Collider projectileCollider;

    // Target filtering
    private GameObject shooter; // Who fired this projectile
    private string targetTag = "Enemy"; // Default target for player projectiles

    public enum StatusEffectType
    {
        None,
        Burn,
        Freeze,
        Slow,
        Stun
    }

    #region Unity Lifecycle

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
        lastPosition = transform.position;

        // Set collision detection mode for fast projectiles
        if (projectileRigidbody != null)
        {
            projectileRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void Update()
    {
        // Track travel distance for damage falloff
        if (damageFalloffDistance > 0)
        {
            travelDistance += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the projectile with damage and shooter information
    /// </summary>
    /// <param name="damage">Base damage amount</param>
    /// <param name="shooterObject">GameObject that fired this projectile</param>
    /// <param name="targetTagOverride">Override for target tag (default: "Enemy")</param>
    public void Initialize(int damage, GameObject shooterObject, string targetTagOverride = null)
    {
        baseDamage = damage;
        shooter = shooterObject;

        if (!string.IsNullOrEmpty(targetTagOverride))
        {
            targetTag = targetTagOverride;
        }
        else
        {
            // Auto-determine target tag based on shooter
            if (shooterObject != null && shooterObject.CompareTag("Player"))
            {
                targetTag = "Enemy";
            }
            else if (shooterObject != null && shooterObject.CompareTag("Enemy"))
            {
                targetTag = "Player";
            }
        }

        Debug.Log($"Projectile initialized - Damage: {baseDamage}, Shooter: {shooterObject?.name}, Target: {targetTag}");
    }

    /// <summary>
    /// Set projectile type and adjust settings accordingly
    /// </summary>
    /// <param name="type">Type of projectile</param>
    public void SetProjectileType(ProjectileType type)
    {
        projectileType = type;

        // Auto-configure based on projectile type
        switch (type)
        {
            case ProjectileType.Arrow:
                damageMode = DamageMode.SingleTarget;
                canKnockback = true;
                knockbackForce = 3f;
                break;

            case ProjectileType.Fireball:
                damageMode = DamageMode.AreaOfEffect;
                aoeRadius = 3f;
                canApplyStatusEffect = true;
                statusEffectType = StatusEffectType.Burn;
                statusEffectDuration = 5f;
                break;

            case ProjectileType.IceBlast:
                damageMode = DamageMode.AreaOfEffect;
                aoeRadius = 4f;
                canApplyStatusEffect = true;
                statusEffectType = StatusEffectType.Freeze;
                statusEffectDuration = 3f;
                break;

            case ProjectileType.Lightning:
                damageMode = DamageMode.Piercing;
                maxPierceTargets = 5;
                damageLossPerPierce = 0.05f;
                break;
        }
    }

    #endregion

    #region Collision Detection

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider hitCollider)
    {
        // Ignore collision with shooter
        if (hitCollider.gameObject == shooter)
            return;

        // Check if we should damage this target
        bool isValidTarget = ShouldDamageTarget(hitCollider.gameObject);

        if (isValidTarget)
        {
            ProcessDamage(hitCollider);
        }
        else if (hitCollider.gameObject.layer != LayerMask.NameToLayer("Projectile"))
        {
            // Hit something else (wall, obstacle, etc.)
            ProcessNonTargetHit(hitCollider.transform.position);
        }
    }

    private bool ShouldDamageTarget(GameObject target)
    {
        // Check layer mask
        if (((1 << target.layer) & damageLayerMask) == 0)
            return false;

        // Check tag
        if (!target.CompareTag(targetTag))
            return false;

        // Check if target has a health component
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null)
        {
            // Also check for EnemyHealth for backward compatibility
            var enemyHealth = target.GetComponent<Health>();
            if (enemyHealth == null)
                return false;
        }

        return true;
    }

    #endregion

    #region Damage Processing

    private void ProcessDamage(Collider hitTarget)
    {
        Vector3 hitPosition = GetClosestPointOnCollider(hitTarget);

        switch (damageMode)
        {
            case DamageMode.SingleTarget:
                DamageSingleTarget(hitTarget.gameObject, hitPosition);
                DestroyProjectile(hitPosition);
                break;

            case DamageMode.AreaOfEffect:
                DamageAreaOfEffect(hitPosition);
                DestroyProjectile(hitPosition);
                break;

            case DamageMode.Piercing:
                DamageSingleTarget(hitTarget.gameObject, hitPosition);
                HandlePiercing();
                break;
        }
    }

    private void DamageSingleTarget(GameObject target, Vector3 hitPosition)
    {
        int finalDamage = CalculateFinalDamage(hitPosition);
        ApplyDamageToTarget(target, finalDamage, hitPosition);

        // Apply knockback if enabled
        if (canKnockback)
        {
            ApplyKnockback(target, hitPosition);
        }

        OnProjectileHit?.Invoke(target);
    }

    private void DamageAreaOfEffect(Vector3 explosionCenter)
    {
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, aoeRadius, damageLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            if (!ShouldDamageTarget(hitCollider.gameObject))
                continue;

            float distanceFromCenter = Vector3.Distance(explosionCenter, hitCollider.transform.position);
            float damageMultiplier = 1f - (distanceFromCenter / aoeRadius);
            damageMultiplier = Mathf.Clamp01(damageMultiplier);

            int finalDamage = Mathf.RoundToInt(CalculateFinalDamage(explosionCenter) * damageMultiplier);
            ApplyDamageToTarget(hitCollider.gameObject, finalDamage, explosionCenter);

            // Apply area knockback
            if (canKnockback && distanceFromCenter <= knockbackRadius)
            {
                ApplyKnockback(hitCollider.gameObject, explosionCenter);
            }

            OnProjectileHit?.Invoke(hitCollider.gameObject);
        }

        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionCenter, Quaternion.identity);
            Destroy(explosion, effectLifetime);
        }
    }

    private void HandlePiercing()
    {
        pierceCount++;

        if (pierceCount >= maxPierceTargets)
        {
            DestroyProjectile(transform.position);
        }
    }

    #endregion

    #region Damage Calculation and Application

    private int CalculateFinalDamage(Vector3 hitPosition)
    {
        float damage = baseDamage;

        // Apply damage falloff based on distance traveled
        if (damageFalloffDistance > 0 && travelDistance > damageFalloffDistance)
        {
            float falloffMultiplier = Mathf.Lerp(1f, minDamageMultiplier,
                (travelDistance - damageFalloffDistance) / damageFalloffDistance);
            damage *= falloffMultiplier;
        }

        // Apply pierce damage reduction
        if (damageMode == DamageMode.Piercing && pierceCount > 0)
        {
            float pierceMultiplier = 1f - (damageLossPerPierce * pierceCount);
            pierceMultiplier = Mathf.Max(0.1f, pierceMultiplier); // Minimum 10% damage
            damage *= pierceMultiplier;
        }

        return Mathf.RoundToInt(damage);
    }

    private void ApplyDamageToTarget(GameObject target, int damage, Vector3 hitPosition)
    {
        // Try Health component first (new system)
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
        else
        {
            // Fallback to EnemyHealth (old system)
            var enemyHealth = target.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Apply status effects
        if (canApplyStatusEffect && Random.Range(0f, 1f) <= statusEffectChance)
        {
            ApplyStatusEffect(target);
        }

        // Spawn hit effect
        SpawnHitEffect(hitPosition);

        // Play hit sound
        PlayHitSound();

        Debug.Log($"Projectile dealt {damage} damage to {target.name}");
    }

    #endregion

    #region Effects and Status

    private void ApplyKnockback(GameObject target, Vector3 hitPosition)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 knockbackDirection = (target.transform.position - hitPosition).normalized;
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }

    private void ApplyStatusEffect(GameObject target)
    {
        // This is a placeholder - you'd implement actual status effect system
        switch (statusEffectType)
        {
            case StatusEffectType.Burn:
                Debug.Log($"{target.name} is burning!");
                // StartCoroutine(ApplyBurnEffect(target));
                break;
            case StatusEffectType.Freeze:
                Debug.Log($"{target.name} is frozen!");
                // Apply freeze logic
                break;
            case StatusEffectType.Slow:
                Debug.Log($"{target.name} is slowed!");
                // Apply slow logic
                break;
        }
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }
    }

    private void PlayHitSound()
    {
        if (hitSoundEffect != null)
        {
            AudioSource.PlayClipAtPoint(hitSoundEffect, transform.position);
        }
    }

    #endregion

    #region Utility Methods

    private Vector3 GetClosestPointOnCollider(Collider collider)
    {
        return collider.ClosestPoint(transform.position);
    }

    private void ProcessNonTargetHit(Vector3 hitPosition)
    {
        // Hit a wall or other obstacle
        if (damageMode == DamageMode.AreaOfEffect)
        {
            // Still do AOE damage even if we hit a wall
            DamageAreaOfEffect(hitPosition);
        }

        DestroyProjectile(hitPosition);
    }

    private void DestroyProjectile(Vector3 destroyPosition)
    {
        if (hasHit) return; // Prevent multiple destructions

        hasHit = true;
        OnProjectileDestroyed?.Invoke(destroyPosition);

        // Spawn final effect if needed
        SpawnHitEffect(destroyPosition);

        Destroy(gameObject);
    }

    #endregion

    #region Debug and Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showAoeGizmo) return;

        // Draw AOE radius
        if (damageMode == DamageMode.AreaOfEffect)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }

        // Draw knockback radius
        if (canKnockback)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, knockbackRadius);
        }
    }

    #endregion
}