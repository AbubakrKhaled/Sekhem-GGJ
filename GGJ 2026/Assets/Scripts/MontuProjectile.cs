using UnityEngine;

/// <summary>
/// Projectile thrown by Montu boss
/// High damage, requires dodging (dash mechanic)
/// </summary>
public class MontuProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float knockbackForce;
    private float lifetime = 6f; // Long range projectile

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
        // Ignore anything except player
        if (hit.CompareTag("Player"))
        {
            PlayerHealth hp = hit.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                // Deal massive damage with knockback
                hp.TakeDamage(damage, direction);
                
                Debug.Log($"💥 Montu's projectile hit! {damage} damage dealt!");
                
                // Destroy projectile on hit
                Destroy(gameObject);
            }
        }
        else if (!hit.isTrigger)
        {
            // Hit wall, destroy
            Destroy(gameObject);
        }
    }
}
