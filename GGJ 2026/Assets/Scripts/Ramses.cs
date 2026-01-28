using UnityEngine;

public class Ramses : Boss
{
    [Header("Ramses Stats")]
    public float moveSpeed = 2.5f;  // Slower than player but relentless
    public float meleeRange = 0.3f;  // TINY range to allow player close combat
    public float attackCooldown = 1.5f;  // Time between attacks
    
    private float lastAttackTime = -10f;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isFacingRight = true;
    private bool isBack = false;

    public override void Start()
    {
        // --- BOSS STATS ---
        // Player has 100 HP
        // Ramses does 20 damage = kills player in 5 hits
        // Ramses has 300 HP = 3x player health
        baseHealth = 300f;
        baseDamage = 20f;
        enemyName = "Ramses II";

        // Set movement and combat stats (these exist in Enemies base class)
        speed = moveSpeed;
        attackSpeed = attackCooldown;
        attackRange = meleeRange;  // Use inherited attackRange from Enemies

        // Run the boss initialization (UI health bar setup)
        base.Start();

        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        // FORCE VISIBILITY
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10; // High order to be on top
        }
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

        // --- AI BEHAVIOR ---
        bool isMoving = false;
        if (distanceToPlayer <= attackRange)
        {
            // In attack range - stop and attack
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

    public override void Attack()
    {
        // Safety check
        if (player == null) return;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        
        if (hp != null)
        {
            // Calculate push direction (away from Ramses)
            Vector2 hitDirection = (player.position - transform.position).normalized;
            
            // Deal damage (20 damage = kills player in 5 hits)
            hp.TakeDamage((int)currentDamage, hitDirection);
            
            Debug.Log($"Ramses II struck player for {currentDamage} damage!");

            // Trigger animation
            if (anim != null)
            {
                anim.SetTrigger("attack");
            }
        }
    }

    public override void Die()
    {
        Debug.Log("Ramses II has been defeated!");
        // Add boss death effects here (particles, sound, etc.)
        base.Die();
    }
}