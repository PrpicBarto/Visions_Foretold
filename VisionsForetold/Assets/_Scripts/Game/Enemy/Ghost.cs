using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //common ranged enemy (3 verzije)

    public class Ghost : BaseEnemy
    {
        public enum GhostType
        {
            Basic,
            Elite,
            Phantom
        }

        [Header("Ghost Settings")] [SerializeField]
        private GhostType ghostType = GhostType.Basic;

        [SerializeField] private GameObject soulBlastPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private int soulBlastDamage = 12;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float attackCooldown = 2f;

        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            if (firePoint != null) firePoint = transform;
            ConfigureGhostType();
        }

        private void ConfigureGhostType()
        {
            switch (ghostType)
            {
                case GhostType.Basic:
                    soulBlastDamage = 12;
                    attackCooldown = 2f;
                    if (health != null) health.SetMaxHealth(25, false);
                    break;
                case GhostType.Elite:
                    soulBlastDamage = 18;
                    attackCooldown = 1.5f;
                    if (health != null) health.SetMaxHealth(35, false);
                    break;
                case GhostType.Phantom:
                    soulBlastDamage = 15;
                    attackCooldown = 1f;
                    if (health != null) health.SetMaxHealth(20, false);
                    break;
            }
        }

        protected override void UpdateBehavior(float distanceToPlayer)
        {
            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else if (distanceToPlayer < minDistance)
            {
                // Retreat
                Vector3 retreatDir = (transform.position - player.position).normalized;
                agent.SetDestination(transform.position + retreatDir * 2f);
            }
            else
            {
                agent.SetDestination(transform.position);
                LookAtPlayer();
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                ShootSoulBlast();
                lastAttackTime = Time.time;
            }
        }

        private void ShootSoulBlast()
        {
            if (soulBlastPrefab == null) return;

            Vector3 direction = (player.position - firePoint.position).normalized;
            GameObject projectile = Instantiate(soulBlastPrefab, firePoint.position,
                Quaternion.LookRotation(direction));

            ProjectileDamage projDamage = projectile.GetComponent<ProjectileDamage>();
            if (projDamage != null)
            {
                projDamage.Initialize(soulBlastDamage, gameObject, "Player");
                projDamage.SetProjectileType(ProjectileDamage.ProjectileType.EnemyProjectile);
            }
        }
    }
}


    
