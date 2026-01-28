using UnityEngine;

/// <summary>
/// Water projectile for Mage Level 2
/// Single-target homing/straight projectile with higher damage than Earth
/// </summary>
public class WaterProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float knockbackForce;
    private float lifetime = 5f;

    public void Setup(Vector2 dir, float spd, int dmg, float knockback)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        knockbackForce = knockback;
    }

    void Update()
    {
        // Move in direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D hit)
    {
        // Ignore player
        if (hit.CompareTag("Player")) return;

        // Check for enemy
        Enemies enemy = hit.GetComponentInParent<Enemies>();

        if (enemy != null)
        {
            // Deal damage with knockback
            enemy.TakeDamage(damage, knockbackForce, direction);

            // Destroy projectile on hit
            Destroy(gameObject);
        }
        else if (!hit.isTrigger)
        {
            // Hit wall, destroy
            Destroy(gameObject);
        }
    }
}
