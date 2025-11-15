using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Health health;
    
    [Header("Combat")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime;
    
    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float detectionRange = 10f;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (health.IsDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= detectionRange)
        {
            agent.speed = chaseSpeed;
            
            if (distance > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position);
                TryAttack();
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
        }
    }
}