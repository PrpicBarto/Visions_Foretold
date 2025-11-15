using System.Collections;
using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //lich - rare summoner enemy 
    public class Lich : BaseEnemy
    {
        [Header("Lich Settings")]
    [SerializeField] private GameObject ghoulPrefab;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject[] uncommonEnemyPrefabs;
    [SerializeField] private Transform[] summonPoints;
    [SerializeField] private GameObject hellfirePrefab;
    [SerializeField] private int hellfireDamage = 35;
    [SerializeField] private float summonCooldown = 10f;
    [SerializeField] private float attackCooldown = 3f;
    
    private float lastSummonTime;
    private float lastAttackTime;
    private bool isInLastStand;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 2f;
        if (health != null) health.SetMaxHealth(120, false);
        if (agent != null) agent.speed = moveSpeed;
    }

    protected override void UpdateBehavior(float distanceToPlayer)
    {
        // Check for last stand
        if (!isInLastStand && health != null && health.HealthPercentage <= 0.25f)
        {
            StartLastStand();
            return;
        }
        
        // Keep distance away from player
        if (distanceToPlayer < 8f)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + retreatDir * 3f);
        }
        
        // Summon minions
        if (Time.time - lastSummonTime > summonCooldown)
        {
            SummonMinions();
            lastSummonTime = Time.time;
        }
        
        // Attack with hellfire
        if (Time.time - lastAttackTime > attackCooldown && distanceToPlayer <= 12f)
        {
            CastHellfire();
            lastAttackTime = Time.time;
        }
    }

    private void SummonMinions()
    {
        if (summonPoints.Length == 0) return;
        
        // Summon 2 ghouls
        for (int i = 0; i < 2; i++)
        {
            if (ghoulPrefab != null && i < summonPoints.Length)
            {
                Instantiate(ghoulPrefab, summonPoints[i].position, Quaternion.identity);
            }
        }
        
        // Summon 2 ghosts
        for (int i = 2; i < 4; i++)
        {
            if (ghostPrefab != null && i < summonPoints.Length)
            {
                Instantiate(ghostPrefab, summonPoints[i].position, Quaternion.identity);
            }
        }
        
        // Summon 1 random uncommon
        if (uncommonEnemyPrefabs.Length > 0 && summonPoints.Length > 4)
        {
            GameObject randomEnemy = uncommonEnemyPrefabs[Random.Range(0, uncommonEnemyPrefabs.Length)];
            Instantiate(randomEnemy, summonPoints[4].position, Quaternion.identity);
        }
        
        Debug.Log("Lich summoned minions!");
    }

    private void CastHellfire()
    {
        LookAtPlayer();
        
        if (hellfirePrefab != null)
        {
            GameObject hellfire = Instantiate(hellfirePrefab, player.position, Quaternion.identity);
            
            // Deal damage to player
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(hellfireDamage);
            }
            
            Destroy(hellfire, 2f);
        }
    }

    private void StartLastStand()
    {
        isInLastStand = true;
        StartCoroutine(LastStandSwordRain());
    }

    private IEnumerator LastStandSwordRain()
    {
        for (int i = 0; i < 10; i++)
        {
            // Spawn ghost swords in AOE
            Vector3 randomPos = player.position + Random.insideUnitSphere * 5f;
            randomPos.y = player.position.y + 5f;
            
            // TODO: Create sword projectile that falls down
            Debug.Log($"Ghost sword spawned at {randomPos}");
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    }
}