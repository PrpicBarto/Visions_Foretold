using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] float waitingTime;
    private int currentWaypoint = -1;
    private float time;

    [Header("Attacking")]
    [SerializeField] private float spottingDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackSpeed;
    [SerializeField] private int damage;
    private bool canAttack = true;

    [Header("Target")]
    [SerializeField] private Transform player;

    //[Header("Animation")]
    //[SerializeField] private Animator animator;

    private void Start()
    {
        time = waitingTime;
        SetTargetDestination(GetNextWaypoint());
        //animator.fireEvents = false;
    }

    void FixedUpdate()
    {
        //animator.SetFloat("Blend", agent.velocity.magnitude);

        if (player != null)
        {
            float distance = Vector3.Distance(player.position, agent.transform.position);

            if (distance <= spottingDistance)
            {
                SetTargetDestination(player.position);
            }

            else if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    time -= Time.deltaTime;

                    if (time <= 0)
                    {
                        SetTargetDestination(GetNextWaypoint());
                    }
                }
            }

            if (canAttack)
            {
                if (distance <= attackDistance)
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;

        LookAtPlayer();
        //animator.SetTrigger("Attack");

        player.GetComponent<Health>().DealDamage(damage);

        yield return new WaitForSeconds(attackSpeed);

        canAttack = true;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        agent.updateRotation = false;
        transform.rotation = Quaternion.LookRotation(direction);
        agent.updateRotation = true;
    }

    private void SetTargetDestination(Vector3 target)
    {
        agent.SetDestination(target);
    }

    private Vector3 GetNextWaypoint()
    {
        currentWaypoint++;

        if (currentWaypoint >= waypoints.Length)
            currentWaypoint = 0;

        time = waitingTime;
        return waypoints[currentWaypoint].position;
    }
}

















