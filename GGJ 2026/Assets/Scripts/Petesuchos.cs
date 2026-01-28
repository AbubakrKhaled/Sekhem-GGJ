using UnityEngine;

/// <summary>
/// Level 2 Boss: Petesuchos - Demi-God Crocodile
/// Features:
/// - Melee punch attacks
/// - Summons sky beams at random locations
/// - Triple player size (set in prefab scale)
/// - 500 HP (5x player)
/// - 20 damage (kills player in 5 hits)
/// </summary>
public class Petesuchos : Boss
{
    [Header("Petesuchos Stats")]
    public float moveSpeed = 2.0f;         // Slower due to larger size
    public float meleeRange = 2.5f;        // Larger melee range (bigger size)
    public float attackCooldown = 2.0f;    // Slower attacks than Ramses
    
    [Header("Sky Beam Attack")]
    public GameObject skyBeamPrefab;       // Beam prefab (assign in Inspector)
    public float beamCooldown = 4f;        // Time between beam summons
    public int beamsPerCast = 3;           // Number of beams to spawn
    public float beamSpawnRadius = 8f;     // How far from boss to spawn beams
    
    private float lastAttackTime = -10f;
    private float lastBeamTime = -10f;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isFacingRight = true;
    private bool isBack = false;

    public override void Start()
    {
        // --- BOSS STATS ---
        // Player has 100 HP
        // Petesuchos does 20 damage = kills player in 5 hits
        // Petesuchos has 500 HP = 5x player health
        baseHealth = 500f;
        baseDamage = 20f;
        enemyName = "Petesuchos";

        // Set movement and combat stats
        speed = moveSpeed;
        attackSpeed = attackCooldown;
        attackRange = meleeRange;

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
        
        Debug.Log("⚡ Petesuchos awakened! Beware the sky beams!");
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

        // --- SKY BEAM ATTACK (periodic, independent of melee) ---
        if (Time.time >= lastBeamTime + beamCooldown)
        {
            SummonSkyBeams();
            lastBeamTime = Time.time;
        }

        // --- AI BEHAVIOR ---
        bool isMoving = false;
        if (distanceToPlayer <= attackRange)
        {
            // In attack range - stop and punch
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Out of range - chase the player
            ChasePlayer();
            isMoving = true;
        }
        
        // --- ANIMATION ---
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isBack", isBack);
        }
    }
    void ChasePlayer()
    {
        // Move towards player using 2D movement
        Vector2 direction = (player.position - transform.position).normalized;
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
    /// Melee punch attack - damages player on contact
    /// </summary>
    public override void Attack()
    {
        // Safety check
        if (player == null) return;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        
        if (hp != null)
        {
            // Calculate push direction (away from Petesuchos)
            Vector2 hitDirection = (player.position - transform.position).normalized;
            
            // Deal damage (20 damage = kills player in 5 hits)
            hp.TakeDamage((int)currentDamage, hitDirection);
            
            Debug.Log($"💥 Petesuchos punched player for {currentDamage} damage!");

            if (anim != null) anim.SetTrigger("attack");
        }
    }

    /// <summary>
    /// Summon sky beams at random locations around the arena
    /// </summary>
    void SummonSkyBeams()
    {
        if (anim != null) anim.SetTrigger("attack"); // Use same trigger for now

        if (skyBeamPrefab == null)
        {
            Debug.LogWarning("Sky Beam prefab not assigned to Petesuchos!");
            return;
        }

        for (int i = 0; i < beamsPerCast; i++)
        {
            // Random position around boss (or player for more threat)
            Vector2 targetPos;
            
            if (player != null && Random.value > 0.5f)
            {
                // 50% chance to target near player (more dangerous)
                targetPos = (Vector2)player.position + Random.insideUnitCircle * 2f;
            }
            else
            {
                // 50% chance to spawn randomly around boss
                targetPos = (Vector2)transform.position + Random.insideUnitCircle * beamSpawnRadius;
            }

            // Spawn sky beam
            GameObject beam = Instantiate(skyBeamPrefab, targetPos, Quaternion.identity);
            
            // Pass damage to beam if it has the script
            SkyBeam beamScript = beam.GetComponent<SkyBeam>();
            if (beamScript != null)
            {
                beamScript.Setup((int)currentDamage / 2, 3f); // Half damage, moderate knockback
            }
        }

        Debug.Log($"⚡ Petesuchos summoned {beamsPerCast} sky beams!");
    }

    public override void Die()
    {
        Debug.Log("⚡ Petesuchos has been defeated!");
        // Add boss death effects here (particles, sound, etc.)
        base.Die();
    }
}
