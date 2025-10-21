using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : Enemy
{
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int damage = 15;
    public float attackRange = 8f;
    public float attackCooldown = 2f;
    private float lastAttackTime;
    private float moveSpeed;
    
    private NavMeshAgent agent;

    private void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > attackRange)
        {
            agent.SetDestination(player.position);
        }
        else if (distance < attackRange - 2f)
        {
            Vector3 direction = player.position - transform.position;
            Vector3 newPosition = transform.position + direction.normalized * (Time.deltaTime * moveSpeed);
            agent.SetDestination(newPosition);
        }
        else
        {
            agent.SetDestination(transform.position);
            if (Time.time - lastAttackTime > attackCooldown)
            {
                Shoot();
                lastAttackTime = Time.time;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().SetDamage(damage);
    }
}
