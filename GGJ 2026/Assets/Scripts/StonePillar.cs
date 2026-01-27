using UnityEngine;

public class StonePillar : MonoBehaviour
{
    public float damage = 15f;
    public float lifetime = 0.8f;
    public float knockUpForce = 4f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemies enemy = other.GetComponent<Enemies>();
        if (enemy == null) return;

        // deal damage
        enemy.TakeDamage(damage);

        // optional knock-up
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * knockUpForce, ForceMode2D.Impulse);
        }
    }
}
