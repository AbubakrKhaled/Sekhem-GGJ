using UnityEngine;

public class StonePillar : MonoBehaviour
{
    [Header("Pillar Stats")]
    public int damage = 10;              // Can be set by MageMask
    public float knockbackForce = 30f;   // Knockback strength
    public float lifetime = 3.0f;        // How long pillar lasts

    private void Start()
    {
        // Destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Ignore the player (don't hit yourself)
        if (other.CompareTag("Player")) return;

        // 2. Check if it's an enemy
        Enemies enemy = other.GetComponent<Enemies>();
        
        if (enemy != null)
        {
            // Vector from pillar to enemy = knockback direction (radial)
            Vector2 direction = (other.transform.position - transform.position).normalized;

            // 3. Deal damage AND apply knockback
            enemy.TakeDamage(damage, knockbackForce, direction);
        }
    }
}