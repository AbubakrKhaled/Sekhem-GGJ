using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private float speed;
    private float direction; // 1 for Right, -1 for Left
    private int damage;
    private float pushForce;
    private float lifeTime = 5f; // Destroy after 5 seconds if it hits nothing

    public void Setup(float dir, float spd, int dmg, float push)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        pushForce = push;

        // Rotate visual: If moving left, flip the arrow 180 degrees
        if (direction < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    void Update()
    {
        // Move strictly horizontal
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

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
            // Create a horizontal knockback vector
            Vector2 knockDir = new Vector2(direction, 0);

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