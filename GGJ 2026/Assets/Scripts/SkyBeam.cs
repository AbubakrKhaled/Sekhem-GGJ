using UnityEngine;

/// <summary>
/// Sky Beam - Summoned by Petesuchos boss
/// Warns player with indicator, then strikes down dealing damage
/// </summary>
public class SkyBeam : MonoBehaviour
{
    [Header("Beam Stats")]
    public int damage = 10;
    public float knockbackForce = 3f;
    
    [Header("Timing")]
    public float warningDuration = 1.0f;  // Time before beam strikes
    public float strikeLayered = 0.3f;     // How long beam damage lingers
    
    private float spawnTime;
    private bool hasStruck = false;
    private SpriteRenderer spriteRenderer;

    public void Setup(int dmg, float knockback)
    {
        damage = dmg;
        knockbackForce = knockback;
    }

    void Start()
    {
        spawnTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start with warning visual (faded)
        if (spriteRenderer != null)
        {
            Color warningColor = spriteRenderer.color;
            warningColor.a = 0.3f; // Semi-transparent warning
            spriteRenderer.color = warningColor;
        }
    }

    void Update()
    {
        float elapsed = Time.time - spawnTime;

        if (!hasStruck && elapsed >= warningDuration)
        {
            // Warning period over - STRIKE!
            Strike();
            hasStruck = true;
        }

        if (hasStruck && elapsed >= warningDuration + strikeLayered)
        {
            // Beam finished - destroy
            Destroy(gameObject);
        }
    }

    void Strike()
    {
        // Make beam fully visible
        if (spriteRenderer != null)
        {
            Color strikeColor = spriteRenderer.color;
            strikeColor.a = 1f; // Fully opaque
            spriteRenderer.color = strikeColor;
        }

        Debug.Log("⚡ Sky beam struck!");
        
        // Damage is handled in OnTriggerStay2D since beam lingers
    }

    private void OnTriggerStay2D(Collider2D hit)
    {
        // Only damage after strike
        if (!hasStruck) return;

        // Check if player
        if (hit.CompareTag("Player"))
        {
            PlayerHealth hp = hit.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                // Beam pushes player outward from center
                Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
                
                hp.TakeDamage(damage, hitDirection);
                
                // Destroy beam after hitting player (one-time damage)
                Destroy(gameObject);
            }
        }
    }
}
