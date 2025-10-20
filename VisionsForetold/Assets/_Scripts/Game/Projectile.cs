using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player hit for " + damage);
            Destroy(gameObject);
        }
        else
        {
            if (other.CompareTag("Obstacle"))
            {
                Destroy(gameObject);
            }
        }
    }
}
