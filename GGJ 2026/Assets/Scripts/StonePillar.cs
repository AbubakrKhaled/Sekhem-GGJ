using UnityEngine;

public class StonePillar : MonoBehaviour
{
    public float damage = 15f;
    
    // fixed: increased from 0.8 to 3.0 so they stay longer
    public float lifetime = 3.0f; 
    public float knockUpForce = 4f;

    private void Start()
    {
        // destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

    {
        // 1. Ignore the Player (Don't hit yourself)
        if (other.CompareTag("Player")) return;

        Debug.Log("Pillar touched object: " + other.name);

        Enemies enemy = other.GetComponent<Enemies>();
        if (enemy == null) return;

        // ... rest of damage code ...
        enemy.TakeDamage(damage);
    }
        // optional knock-up physics
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * knockUpForce, ForceMode2D.Impulse);
        }
    }
}