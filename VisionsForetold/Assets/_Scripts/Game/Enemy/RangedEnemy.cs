using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Health health;
    [SerializeField] private Transform firePoint;
    
    [Header("Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int damage = 15;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 12f;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        if (firePoint == null) firePoint = transform;
    }

    private void Update()
    {
        if (health.IsDead || player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        
        if (distance <= detectionRange)
        {
            agent.speed = moveSpeed;
            
            if (distance > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else if (distance < minDistance)
            {
                Vector3 direction = (transform.position - player.position).normalized;
                Vector3 retreatPos = transform.position + direction * 2f;
                agent.SetDestination(retreatPos);
            }
            else
            {
                agent.SetDestination(transform.position);
                LookAtPlayer();
                TryShoot();
            }
        }
    }

    private void TryShoot()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            Shoot();
            lastAttackTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;
        
        Vector3 direction = (player.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, 
            Quaternion.LookRotation(direction));
        
        ProjectileDamage projDamage = projectile.GetComponent<ProjectileDamage>();
        if (projDamage != null)
        {
            projDamage.Initialize(damage, gameObject, "Player");
            projDamage.SetProjectileType(ProjectileDamage.ProjectileType.EnemyProjectile);
        }
        
        Debug.Log($"{gameObject.name} shot projectile!");
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}