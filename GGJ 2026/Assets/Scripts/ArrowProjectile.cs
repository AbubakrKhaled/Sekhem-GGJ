using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private float speed;
    private Vector2 direction; // 2D direction for spread shots
    private int damage;
    private float pushForce;
    private float lifeTime = 5f; // Destroy after 5 seconds if it hits nothing

    // New Setup signature for spread shots (2D direction)
    public void Setup(float dirX, float spd, int dmg, float push, float dirY = 0f)
    {
        direction = new Vector2(dirX, dirY).normalized;
        speed = spd;
        damage = dmg;
        pushForce = push;

        // Rotate arrow to face direction of travel
        if (dirY != 0f || dirX < 0)
        {
            float angle = Mathf.Atan2(dirY, dirX) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void Update()
    {
        // Move in 2D direction (supports spread shots)
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Safety cleanup
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D hit)
    {
        // 1. Ignore the Player (shooter)
        if (hit.CompareTag("Player")) return;

        // 2. Check for Enemy (Parent or Child)
        Enemies enemy = hit.GetComponentInParent<Enemies>();

        if (enemy != null)
        {
            // Use the arrow's direction for knockback (supports spread shots)
            Vector2 knockDir = direction.normalized;

            // Deal damage using the method from your Enemies.cs
            enemy.TakeDamage(damage, pushForce, knockDir);

            // Destroy arrow on impact
            Destroy(gameObject);
        }
        else if (!hit.isTrigger)
        {
            // If we hit a wall (non-trigger collider), destroy the arrow
            Destroy(gameObject);
        }
    }
}