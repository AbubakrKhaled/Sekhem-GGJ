using UnityEngine;

public class StonePillar : MonoBehaviour
{
    public float damage = 15f;
    
    // fixed: increased from 0.8 to 3.0 so they stay longer
    public float lifetime = 3.0f; 
    
    // renamed to 'knockBack' to be more generic (works for any direction)
    public float knockBack = 30f; 

    private void Start()
    {
        // destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ignore the player (don't hit yourself)
        if (other.CompareTag("Player")) return;

        // debug check to see what we are hitting
        // Debug.Log("pillar touched object: " + other.name);

        // 2. check if it's an enemy
        Enemies enemy = other.GetComponent<Enemies>();
        
        if (enemy != null)
        {
            // --- DIRECTION CALCULATION ---
            // vector math: (enemy pos - pillar pos) gives the direction AWAY from the pillar center
            Vector2 direction = (other.transform.position - transform.position).normalized;

            // 3. deal damage AND apply knockback
            // we pass the damage, force, and direction to the enemy script
            enemy.TakeDamage(damage, knockBack, direction);
        }
    }
}