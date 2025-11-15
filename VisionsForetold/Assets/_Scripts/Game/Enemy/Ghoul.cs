using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //GHOUL - common melee enemy (3 verzije)
    public class Ghoul : BaseEnemy
    {
        public enum GhoulType
        {
            Basic,
            Strong,
            Fast
        }

        [Header("Ghoul Settings")] 
        [SerializeField] private GhoulType ghoulType = GhoulType.Basic;
        [SerializeField] private int biteDamage = 15;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;

        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            ConfigureGhoulType();
        }

        private void ConfigureGhoulType()
        {
            switch (ghoulType)
            {
                case GhoulType.Basic:
                    biteDamage = 15;
                    moveSpeed = 3f;
                    if (health != null) health.SetMaxHealth(40, false);
                    break;
                case GhoulType.Strong:
                    biteDamage = 25;
                    moveSpeed = 2.5f;
                    if (health != null) health.SetMaxHealth(60, false);
                    break;
                case GhoulType.Fast:
                    biteDamage = 12;
                    moveSpeed = 4.5f;
                    if (health != null) health.SetMaxHealth(30, false);
                    break;
            }

            if (agent != null) agent.speed = moveSpeed;
        }

        protected override void UpdateBehavior(float distanceToPlayer)
        {
            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position);
                TryAttack();
            }
        }

        private void TryAttack()
        {
            LookAtPlayer();
            
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage);
                Debug.Log($"Ghoul ({ghoulType}) bit player for {biteDamage} damage!");
            }
        }
    }
}