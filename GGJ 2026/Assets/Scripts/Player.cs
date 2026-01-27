using System.Collections;
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
        MageMask testMask = GetComponent<MageMask>();
        if (testMask != null)
        {
            mask = testMask;
            mask.enabled = true;
            Debug.Log("TEST MODE: Mage Mask Forced ON.");
        }
        else
        {
            Debug.LogError("TEST MODE FAIL: No MageMask script found on Player!");
        }
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
                Vector3 checkPos = new Vector3(transform.position.x, transform.position.y, currentFloor * map.floorHeight);
                if (map.IsAtLedge(checkPos, facingDir))
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
            spr.flipX = move.x < 0;

        //if (anim != null)
        //    anim.SetFloat("Speed", move.sqrMagnitude);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (map == null) return;

            Vector3 checkPos = new Vector3(transform.position.x, transform.position.y, currentFloor * map.floorHeight);

            if (map.CanJumpUp(checkPos))
            {
                currentFloor++;
                Debug.Log("Jumped to floor: " + currentFloor);
            }
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

            Vector3 checkPos = new Vector3(transform.position.x, transform.position.y, currentFloor * map.floorHeight);
            Vector3? target = map.CanDashAcross(checkPos, facingDir);

            if (target.HasValue)
            {
                // Can dash - teleport to target (same floor)
                Vector3 targetPos = target.Value;
                StartCoroutine(DashToTarget(new Vector2(targetPos.x, targetPos.y)));
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
        else if (Input.GetMouseButtonDown(1))
        {
            mask.CastSecondary();
        }
    }


    // === Coroutines ===
    IEnumerator DashToTarget(Vector2 target)
    {
        isDashing = true;
        isInvulnerable = true;

        // Disable collision with enemies (2D version)
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
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
                break;
        }
    }
}