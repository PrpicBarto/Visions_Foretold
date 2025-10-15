using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform player;

    private void FixedUpdate()
    {
        FollowPlayerOnSpawn();
    }

    void FollowPlayerOnSpawn()
    {
        agent.SetDestination(player.position);
    }
}
