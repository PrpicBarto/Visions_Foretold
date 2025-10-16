using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField] GameObject ragdoll;
    public bool isPlayer;

    public void DealDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            GameObject ragdollClone = Instantiate(ragdoll);
            ragdollClone.transform.position = transform.position;

            if (isPlayer)
            {
                //TODO: ADD PLAYER DEATH LOGIC  
            }

            Destroy(gameObject);
        }
    }

    public void AddHealth(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }
}
