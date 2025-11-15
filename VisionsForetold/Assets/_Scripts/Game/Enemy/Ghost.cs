namespace _Scripts.Game.Enemy
{
    public class Ghost : BaseEnemy
    {
        public enum GhostType
        {
            Basic,
            Elite,
            Phantom
        }
        protected override void UpdateBehavior(float distanceToPlayer)
        {
            
        }
    }
}