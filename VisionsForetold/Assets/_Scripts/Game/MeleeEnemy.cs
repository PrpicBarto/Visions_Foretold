using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    public Transform player;
    public int damage = 20;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    private float lastAttackTime;
    
    private NavMeshAgent agent;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        if (Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            lastAttackTime= Time.time;
        }
    }

    void Attack()
    {
        Debug.Log($"Melee enemy attacks for " + damage);
    }
}
