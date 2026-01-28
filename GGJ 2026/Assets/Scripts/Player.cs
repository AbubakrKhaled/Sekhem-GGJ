using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaskType { Melee, Ranged, Mage }
public enum FacingVertical { Front, Back }

public class Player : MonoBehaviour
{
    // === Components ===
    [Header("Components")]
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform Tr;
    [HideInInspector] public SpriteRenderer spr;
    [HideInInspector] public Animator anim;

    [Header("Mask Animators")]
    [SerializeField] private RuntimeAnimatorController meleeAnimator;
    [SerializeField] private RuntimeAnimatorController rangedAnimator;
    [SerializeField] private RuntimeAnimatorController mageAnimator;

    // === Movement ===
    [Header("Movement")]
    public int speed = 5;
    //float dashSpeed = 10f;
    //float dashDuration = 0.5f;
    float dashCooldown = 5f;
    float lastDashTime;
    float currentSpeed;
    Vector2 lastMoveDir = Vector2.right;

    // === State ===
    [HideInInspector] public bool isInvulnerable;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool isKnockedBack;

    // === Jumping ===
    [Header("Jumping")]
    public int jump = 7;
    //private bool isGrounded;

    // === Animations ===
    bool isDashing = false;

    // === Scripts ===
    [Header("Scripts")]
    BaseMask mask;

    // === Level Map ===
    private LevelMap map;
    private Vector2 facingDir = Vector2.right;
    private int currentFloor = 0;
    
    // Jump zone tracking (simple collider-based system)
    private HashSet<int> jumpZonesInRange = new HashSet<int>();
    private bool isJumping = false;
    private bool isGrounded = true;
    
    [Header("Jump Animation")]
    public Sprite jumpSprite; // PLACEHOLDER: Assign jump animation sprite from art team


    public FacingVertical facingVertical { get; private set; } = FacingVertical.Front;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // --- CRITICAL PHYSICS FIXES ---
        // Prevent bouncing at corners and walls
        if (rb != null)
        {
            rb.gravityScale = 0f; // No gravity in top-down view
            rb.freezeRotation = true; // Don't rotate on collision
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
        }

        currentSpeed = speed;

        StartCoroutine(Blink());

        map = LevelMap.Instance;

        if (map != null)
        {
            currentFloor = 0;
        }

        // --- TEST MODE FIX ---
        // Instead of asking GameSession (which is empty), we FORCE the Mage Mask.
        //MageMask testMask = GetComponent<MageMask>();
        //if (testMask != null)
        //{
        //    mask = testMask;
        //    mask.enabled = true;
        //    Debug.Log("TEST MODE: Mage Mask Forced ON.");
        //}
        //else
        //{
        //    Debug.LogError("TEST MODE FAIL: No MageMask script found on Player!");
        //}
        //MeleeMask testMask2 = GetComponent<MeleeMask>(); // Check for Melee first
        //if (testMask2 != null)
        //{
        //    mask = testMask2;
        //    mask.enabled = true;
        //    Debug.Log("TEST MODE: Melee Mask Forced ON.");
        //}

        //ApplyMask(GameSession.SelectedMask);
        mask = GetComponent<MeleeMask>();
        mask.enabled = true;
        
        // Only set animator if it exists
        if (meleeAnimator != null && anim != null)
        {
            anim.runtimeAnimatorController = meleeAnimator;
        }
        else
        {
            Debug.LogWarning("Melee animator controller not assigned! Animations may not work.");
        }


    }

    void Update()
    {
        // ALWAYS update animations, even during knockback
        UpdateAnimations();
        
        // Stop input handling during knockback
        if (isKnockedBack) return;
        
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttacks();
        HandleDrop();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;
        
        // Update movement animation
        bool moving = rb.linearVelocity.sqrMagnitude > 0.01f;
        
        // Only set parameters if they exist in the Animator Controller
        if (HasAnimatorParameter(anim, "isMoving"))
            anim.SetBool("isMoving", moving);
        
        if (HasAnimatorParameter(anim, "isBack"))
            anim.SetBool("isBack", facingVertical == FacingVertical.Back);
    }
    
    // Helper method to check if animator parameter exists
    private bool HasAnimatorParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void HandleMovement()
    {
        // CRITICAL: Don't override velocity during knockback or dash
        if (isDashing || isKnockedBack) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        float isoX = x + y;
        float isoY = y - x;
        Vector2 move = new Vector2(isoX, isoY);

        if (move.sqrMagnitude > 0.01f)
        {
            move = move.normalized;

            facingVertical = isoY > 0 ? FacingVertical.Back : FacingVertical.Front;

            facingDir = new Vector2(
                Mathf.RoundToInt(isoX),
                Mathf.RoundToInt(isoY)
            );

            // Check ledge - auto drop
            if (map != null && currentFloor > 0)
            {
                if (map.IsAtLedge(currentFloor, transform.position, facingDir))
                {
                    currentFloor--;
                    Debug.Log("Dropped to floor: " + currentFloor);
                    return;
                }
            }
            
            // Flip sprite
            if (Mathf.Abs(isoX) > 0.01f)
                spr.flipX = isoX < 0;
        }

        // Apply velocity (2D) - ONLY when not in knockback
        rb.linearVelocity = move * speed;
    }

    //private void HandleJump()
    //{
    //    if (LevelMap.Instance == null)
    //    {
    //        Debug.LogError("LevelMap.Instance is NULL");
    //        return;
    //    }
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (map == null) return;

    //        if (map.CanJumpUp(currentFloor, transform.position))
    //        {
    //            currentFloor++;
    //            Debug.Log("Jumped to floor: " + currentFloor);
    //            if (Audiomanager.Instance != null)
    //                Audiomanager.Instance.PlaySFX(Audiomanager.Instance.jump);
    //        }
    //    }
    //}

    private void HandleJump()
    {
        // REMOVED - No manual jumping! Auto-hurdle on platform contact
    }
    
    IEnumerator JumpAnimation(int targetFloor)
    {
        isJumping = true;
        
        int startFloor = currentFloor;
        float floorHeight = 3.0f; // Y distance between floors
        float targetY = targetFloor * floorHeight;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);
        
        float duration = 0.25f; // Quick hurdle
        float elapsed = 0f;
        float arcHeight = 1.5f; // Lower arc for hurdle feel
        
        // Switch to jump sprite
        Sprite originalSprite = spr.sprite;
        if (jumpSprite != null)
        {
            spr.sprite = jumpSprite;
        }
        
        Debug.Log($"Auto-hurdling {startFloor} → {targetFloor}");
        
        // Animate arc
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lerp to target with arc
            float currentY = Mathf.Lerp(startPos.y, targetY, t);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            
            transform.position = new Vector3(
                startPos.x,
                currentY + arc,
                startPos.z
            );
            
            yield return null;
        }
        
        // Snap to target
        transform.position = targetPos;
        
        // Update floor
        currentFloor = targetFloor;
        spr.sortingOrder = currentFloor * 10;
        
        // Restore sprite
        if (originalSprite != null)
        {
            spr.sprite = originalSprite;
        }
        
        // Sound
        //if (Audiomanager.Instance != null)
        //{
        //    Audiomanager.Instance.PlaySFX(Audiomanager.Instance.jump);
        //}
        
        isJumping = false;
        isGrounded = true;
        
        Debug.Log($"Landed floor {currentFloor}, Y={transform.position.y:F1}");
    }
    
    // AUTO-HURDLE: Jump automatically when entering platform
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("JumpZone"))
        {
            jumpZonesInRange.Add(other.GetInstanceID());
            
            // Auto-hurdle when entering!
            if (!isJumping)
            {
                // Determine direction
                if (currentFloor < 2)
                {
                    StartCoroutine(JumpAnimation(currentFloor + 1));
                }
                else if (currentFloor > 0)
                {
                    StartCoroutine(JumpAnimation(currentFloor - 1));
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("JumpZone"))
        {
            jumpZonesInRange.Remove(other.GetInstanceID());
        }
    }



    private void HandleDash()
    {
        // Shift = dash across OR drop down
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time < lastDashTime + dashCooldown) return;
            lastDashTime = Time.time;

            if (map == null) return;

            Vector3? target = map.CanDashAcross(currentFloor, transform.position, facingDir);

            if (target.HasValue)
            {
                // Can dash - teleport to target (same floor)
                Vector3 targetPos = target.Value;
                StartCoroutine(DashToTarget(new Vector2(targetPos.x, targetPos.y)));
                //if (Audiomanager.Instance != null)
                //    Audiomanager.Instance.PlaySFX(Audiomanager.Instance.dash);

            }
            else
            {
                // Can't dash - drop down
                if (currentFloor > 0)
                {
                    currentFloor--;
                    StartCoroutine(DashInvulnerability());
                    Debug.Log("Dash failed, dropped to floor: " + currentFloor);
                }
            }
        }
    }

    // Duplicate methods removed - using auto-hurdle versions above
    
    // Ground check (prevents wall jumping)
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Check if any contact point is from below (player standing on something)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) // Contact from below
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    Debug.Log("Grounded!");
                }
                return;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if we're leaving ground (not just hitting a wall)
        if (isGrounded)
        {
            isGrounded = false;
            Debug.Log("Airborne");
        }
    }

    private void HandleDrop()
    {
        // Ctrl = drop down one floor
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (currentFloor > 0)
            {
                currentFloor--;
                Debug.Log("Dropped to floor: " + currentFloor);
            }
        }
    }

    private float timeSinceLastAttackReset = 0f;

    private void HandleAttacks()
    {
        // FAILSAFE: If canAttack is stuck false for > 2 seconds (and not dead), reset it
        if (!canAttack)
        {
            timeSinceLastAttackReset += Time.deltaTime;
            if (timeSinceLastAttackReset > 2.0f && !isKnockedBack)
            {
                canAttack = true;
                Debug.LogWarning("FAILSAFE: Reset stuck canAttack flag!");
                timeSinceLastAttackReset = 0f;
            }
            return; // blocked
        }
        else
        {
            timeSinceLastAttackReset = 0f;
        }

        // Safety check
        if (mask == null)
        {
            // If we lost the mask, try to find it again
            mask = GetComponent<BaseMask>();
            if (mask == null) return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Attempting attack with {mask.GetType().Name}...");
            mask.CastPrimary();
        }

    }


    // === Coroutines ===
    IEnumerator DashToTarget(Vector2 target)
    {
        isDashing = true;
        isInvulnerable = true;

        // Disable collision with enemies (2D version)
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // Teleport
        transform.position = new Vector3(target.x, target.y, transform.position.z);

        yield return new WaitForSeconds(0.2f);

        // Re-enable collision
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isInvulnerable = false;
        isDashing = false;
    }

    IEnumerator DashInvulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(0.2f);
        isInvulnerable = false;
    }

    IEnumerator Blink()
    {
        for(int i = 0; i < 5; i++)
        {
            spr.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spr.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public int GetCurrentFloor()
    {
        return currentFloor;
    }

    // Kept this for later use, but Start() overrides it for now
    public void ApplyMask(MaskType type)
    {
        if (mask != null) mask.enabled = false;

        switch (type)
        {
            case MaskType.Mage:
                MageMask existingMage = GetComponent<MageMask>();
                if (existingMage != null)
                {
                    mask = existingMage;
                    mask.enabled = true;
                    anim.runtimeAnimatorController = mageAnimator;

                }
                else
                {
                    mask = gameObject.AddComponent<MageMask>();
                }
                Debug.Log("Mage Mask Applied");
                break;

            case MaskType.Ranged:
                RangerMask existingRanged = GetComponent<RangerMask>();
                if (existingRanged != null)
                {
                    mask = existingRanged;
                    mask.enabled = true;
                    anim.runtimeAnimatorController = rangedAnimator;

                }
                else
                {
                    mask = gameObject.AddComponent<RangerMask>();
                }
                Debug.Log("Ranged Mask Applied");
                break;

            case MaskType.Melee:
                MeleeMask existingMelee = GetComponent<MeleeMask>();
                if (existingMelee != null)
                {
                    mask = existingMelee;
                    mask.enabled = true;
                    anim.runtimeAnimatorController = meleeAnimator;

                }
                else
                {
                    mask = gameObject.AddComponent<MeleeMask>();
                }
                Debug.Log("Melee Mask Applied");
                break;
            
        }
    }
}