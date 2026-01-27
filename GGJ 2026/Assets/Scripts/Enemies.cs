using UnityEngine;

public abstract class Enemies : MonoBehaviour
{
    [Header("Base Stats (Level 1 Values)")]
    public string enemyName;
    // set these in the inspector for how strong the enemy is at level 1
    public float baseHealth = 100f;
    public float baseDamage = 10f;
    
    [Header("Scaling Settings")]
    // how much stronger they get per level (0.1 means 10% stronger each level)
    public float healthGrowth = 0.1f; 
    public float damageGrowth = 0.1f;

    [Header("Current Stats (Read Only)")]
    // these are the variables we actually use in the game logic
    public float currentHealth;
    public float currentDamage;
    
    // we keep speed and range constant usually, but you can scale them too if you want
    public float speed = 3f;
    public float attackRange = 1.5f;
    public float attackSpeed = 1f; 

    [HideInInspector]
    public Transform player;

    // --- NEW: needed for physics pushback ---
    protected Rigidbody2D rb; 

    public virtual void Start()
    {
        // 1. grab the rigidbody component so we can be pushed around
        rb = GetComponent<Rigidbody2D>();

        // 2. find the player
        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.transform;
        }

        // 3. calculate stats based on the level
        InitializeStats();
    }

    private void InitializeStats()
    {
        // get the current level from our static tracker
        int level = GameTracker.currentLevel;

        // formula: base + (base * growth * (level - 1))
        currentHealth = baseHealth * (1f + (healthGrowth * (level - 1)));
        currentDamage = baseDamage * (1f + (damageGrowth * (level - 1)));
    }

    // --- UPDATED TAKEDAMAGE FUNCTION ---
    // now accepts optional knockback force and direction variables
    public virtual void TakeDamage(float amount, float knockbackForce = 0f, Vector2 knockbackDir = default)
    {
        // we subtract from currentHealth, not baseHealth
        currentHealth -= amount;
        
        Debug.Log(enemyName + " took " + amount + " damage. Remaining: " + currentHealth);

        // --- PHYSICS LOGIC ---
        // only run this if we actually want to push the enemy
        if (rb != null && knockbackForce > 0)
        {
            // stop current movement so the hit feels heavy and snappy
            rb.linearVelocity = Vector2.zero; 
            
            // apply the push force in the direction requested
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    public abstract void Attack();
}