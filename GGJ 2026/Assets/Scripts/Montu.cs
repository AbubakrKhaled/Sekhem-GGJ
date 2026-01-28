using UnityEngine;

/// <summary>
/// Level 3 Boss: Montu - God of War
/// Features:
/// - Large arms (visual, set in sprite/prefab)
/// - Throws projectiles randomly
/// - 1000 HP (10x player)
/// - 35 damage (kills player in 3 hits despite Level 3 upgrades)
/// </summary>
public class Montu : Boss
{
    [Header("Montu Stats")]
    public float moveSpeed = 1.5f;         // Slowest boss (stationary ranged)
    public float detectionRange = 15f;     // Engages from far away
    
    [Header("Projectile Attack")]
    public GameObject projectilePrefab;    // Projectile prefab (assign in Inspector)
    public float projectileCooldown = 1.5f; // Fast projectile spam
    public float projectileSpeed = 8f;      // Projectile speed
    public int projectilesPerBurst = 1;    // Can be increased for harder difficulty
    public float randomSpreadAngle = 30f;  // Random angle deviation
    
    private float lastProjectileTime = -10f;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isFacingRight = true;
    private bool isBack = false;

    public override void Start()
    {
        // --- BOSS STATS ---
        // Player has ~100 HP (even with upgrades)
        // Montu does 35 damage = kills player in 3 hits
        // Montu has 1000 HP = 10x player health
        baseHealth = 1000f;
        baseDamage = 35f;
        enemyName = "Montu";

        // Set movement and combat stats
        speed = moveSpeed;
        attackSpeed = projectileCooldown;
        attackRange = detectionRange; // Engages from very far

        // Run the boss initialization (UI health bar setup)
        base.Start();

        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        // FORCE VISIBILITY
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }
        
        Debug.Log("⚔️ Montu, God of War has arrived! Dodge or die!");
    }

    void Update()
    {
        // Safety check
        if (player == null) return;

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- FACING DIRECTION ---
        Vector2 toPlayer = player.position - transform.position;
        isBack = toPlayer.y > 0;

        // --- SPRITE FLIPPING ---
        // Face the player
        if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }

        // --- RANGED COMBAT BEHAVIOR ---
        if (distanceToPlayer <= detectionRange)
        {
            // In range - throw projectiles!
            if (Time.time >= lastProjectileTime + projectileCooldown)
            {
                Attack();
                lastProjectileTime = Time.time;
            }
        }
        
        // --- ANIMATION ---
        if (anim != null)
        {
            // Montu is mostly stationary (ranged), isMoving is usually false
            // But we can check if he's chasing if we add chase logic later
            anim.SetBool("isMoving", false); 
            anim.SetBool("isBack", isBack);
        }
    }
    void ChasePlayer()
    {
        // Montu is a ranged boss, but we can add minor positioning logic here if needed
        // For now, he can stay relatively stationary or slowly reposition
        if (player == null) return;
        
        // Move towards player slowly if out of range
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            moveSpeed * Time.deltaTime
        );
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
        else
        {
            // Fallback to scale flipping if no sprite renderer
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// Throw projectiles at player with random spread
    /// </summary>
    public override void Attack()
    {
        if (anim != null) anim.SetTrigger("attack");

        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned to Montu!");
            return;
        }

        if (player == null) return;

        // Throw projectile(s) with random spread
        for (int i = 0; i < projectilesPerBurst; i++)
        {
            // Base direction toward player
            Vector2 baseDirection = (player.position - transform.position).normalized;
            
            // Add random spread
            float randomAngle = Random.Range(-randomSpreadAngle, randomSpreadAngle);
            float angleRad = randomAngle * Mathf.Deg2Rad;
            
            // Rotate direction by random angle
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            Vector2 direction = new Vector2(
                baseDirection.x * cos - baseDirection.y * sin,
                baseDirection.x * sin + baseDirection.y * cos
            );

            // Spawn projectile from boss position (or from "hand" offset)
            Vector3 spawnPos = transform.position + (Vector3)(direction * 1.5f); // Offset from center
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Rotate projectile to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Setup projectile
            MontuProjectile projectileScript = projectile.GetComponent<MontuProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(direction, projectileSpeed, (int)currentDamage, 8f);
            }
        }

        Debug.Log($"⚔️ Montu threw {projectilesPerBurst} projectile(s)!");
    }

    public override void Die()
    {
        Debug.Log("⚔️ Montu, God of War has been defeated! Victory!");
        // Add boss death effects here (particles, sound, etc.)
        base.Die();
    }
}
