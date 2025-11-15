using UnityEngine;
using UnityEngine.AI;

//base klasa - svi bad guys inheritaju ovo

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Health health;
    [SerializeField] protected Animator animator;

    [Header("Base Stats")] 
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float moveSpeed = 3f;

    protected bool isDead = false;

    protected virtual void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();

        FindPlayer();

        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
        }
    }

    protected virtual void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (!isDead || player == null || health.isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            UpdateBehavior(distanceToPlayer);
        }
    }
    
    protected abstract void UpdateBehavior(float distanceToPlayer);
    
    protected virtual void OnDeath()
    {
        isDead = true;
        if (agent != null) agent.enabled = false;
    }

    protected void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
