using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //revenant - uncommon support enemy (2 verzije)
    public class Revenant : BaseEnemy
    {
        public enum RevenantType
        {
            Healer,
            Buffer
        }
    
    [Header("Revenant Settings")]
    [SerializeField] private RevenantType revenantType = RevenantType.Healer;
    [SerializeField] private float supportRange = 10f;
    [SerializeField] private float supportCooldown = 5f;
    [SerializeField] private int healAmount = 15;
    [SerializeField] private float attackBuffMultiplier = 1.5f;
    [SerializeField] private float buffDuration = 10f;
    
    private float lastSupportTime;

    protected override void Awake()
    {
        base.Awake();
        ConfigureRevenantType();
    }

    private void ConfigureRevenantType()
    {
        switch (revenantType)
        {
            case RevenantType.Healer:
                supportCooldown = 5f;
                if (health != null) health.SetMaxHealth(45, false);
                break;
            case RevenantType.Buffer:
                supportCooldown = 8f;
                if (health != null) health.SetMaxHealth(40, false);
                break;
        }
    }

    protected override void UpdateBehavior(float distanceToPlayer)
    {
        // Keep distance from player
        if (distanceToPlayer < 6f)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + retreatDir * 2f);
        }
        else
        {
            agent.SetDestination(transform.position);
        }
        
        // Provide support to nearby allies
        if (Time.time - lastSupportTime > supportCooldown)
        {
            ProvideSupport();
            lastSupportTime = Time.time;
        }
    }

    private void ProvideSupport()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, supportRange);
        
        foreach (var col in nearbyEnemies)
        {
            if (col.CompareTag("Enemy") && col.gameObject != gameObject)
            {
                if (revenantType == RevenantType.Healer)
                {
                    Health enemyHealth = col.GetComponent<Health>();
                    if (enemyHealth != null && !enemyHealth.IsAtFullHealth)
                    {
                        enemyHealth.Heal(healAmount);
                        Debug.Log($"Revenant healed {col.name} for {healAmount}");
                    }
                }
                else // za potencijalni system buffova
                {
                    // Apply attack buff - implementirati buff system
                    Debug.Log($"Revenant buffed {col.name}'s attack!");
                    
                }
            }
        }
    }
    }
}