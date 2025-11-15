using System.Collections;
using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //spectre - rare tank sa last standom
    public class Spectre : BaseEnemy
    { 
        [Header("Spectre Settings")]
    [SerializeField] private float shockwaveRange = 8f;
    [SerializeField] private int shockwaveDamage = 25;
    [SerializeField] private float shockwaveCooldown = 6f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private int chargeDamage = 40;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private GameObject fleshRemnantPrefab;
    
    private float lastShockwaveTime;
    private bool isInLastStand;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 2f;
        if (health != null) health.SetMaxHealth(100, false);
        if (agent != null) agent.speed = moveSpeed;
    }

    protected override void UpdateBehavior(float distanceToPlayer)
    {
        // Check for last stand
        if (!isInLastStand && health != null && health.HealthPercentage <= 0.2f)
        {
            StartLastStand();
            return;
        }
        
        // Normal behavior
        if (distanceToPlayer > 3f)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
            TryShockwave();
        }
    }

    private void TryShockwave()
    {
        if (Time.time - lastShockwaveTime > shockwaveCooldown)
        {
            Shockwave();
            lastShockwaveTime = Time.time;
        }
    }

    private void Shockwave()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, shockwaveRange);
        
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(shockwaveDamage);
                    //TODO: Apply stun effect
                    Debug.Log($"Player stunned for {stunDuration} seconds!");
                }
            }
        }
        
        Debug.Log("Spectre unleashed shockwave!");
    }

    private void StartLastStand()
    {
        isInLastStand = true;
        StartCoroutine(LastStandCharge());
    }

    private IEnumerator LastStandCharge()
    {
        if (agent != null)
        {
            agent.speed = chargeSpeed;
            agent.SetDestination(player.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        // Explode on contact
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < 3f)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(chargeDamage);
            }
        }
        
        // Spawn flesh remnants
        if (fleshRemnantPrefab != null)
        {
            Instantiate(fleshRemnantPrefab, transform.position, Quaternion.identity);
        }
        
        
        if (health != null)
        {
            health.SetHealth(0);
        }
    }
    }
}