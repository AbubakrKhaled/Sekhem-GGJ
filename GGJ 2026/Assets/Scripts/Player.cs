using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaskType { Melee, Ranged, Mage }

public class Player : MonoBehaviour
{
    // === Components ===
    [Header("Components")]
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform Tr;
    [HideInInspector] public SpriteRenderer spr;
    
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


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();

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
        //if (testMask != null)
        //{
        //    mask = testMask2;
        //    mask.enabled = true;
        //    Debug.Log("TEST MODE: Melee Mask Forced ON.");
        //}

        ApplyMask(GameSession.SelectedMask);

    }

    void Update()
    {
        if (isKnockedBack) return;
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttacks();
        HandleDrop();
    }
    
    void OnGUI()
    {
        // DEBUG: Show current floor in top-left corner
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        
        GUI.Label(new Rect(10, 10, 300, 40), $"Floor: {currentFloor}", style);
        
        // Also show grounded status
        style.fontSize = 18;
        style.normal.textColor = isGrounded ? Color.green : Color.red;
        GUI.Label(new Rect(10, 45, 300, 30), $"{(isGrounded ? "Grounded" : "Airborne")}", style);
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        float isoX = x + y;
        float isoY = y - x;
        Vector2 move = new Vector2(isoX, isoY);

        if (move.sqrMagnitude > 0.01f)
        {
            move = move.normalized;

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
        }

        // Apply velocity (2D)
        rb.linearVelocity = move * speed;

        // Flip sprite
        if (move.x != 0)
            spr.flipX = move.x > 0;

        //if (anim != null)
        //    anim.SetFloat("Speed", move.sqrMagnitude);
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
        if (Audiomanager.Instance != null)
        {
            Audiomanager.Instance.PlaySFX(Audiomanager.Instance.jump);
        }
        
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
                if (Audiomanager.Instance != null)
                    Audiomanager.Instance.PlaySFX(Audiomanager.Instance.dash);

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

    private void HandleAttacks()
    {
        if (!canAttack) return;

        // Safety check
        if (mask == null)
        {
            // If we lost the mask, try to find it again
            mask = GetComponent<BaseMask>();
            if (mask == null) return;
        }

        if (Input.GetMouseButtonDown(0))
        {
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