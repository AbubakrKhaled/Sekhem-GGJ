using System.Collections;
using UnityEngine;


public enum MaskType { Melee, Ranged, Mage }
//public MaskType currentMask;


public class Player : MonoBehaviour
{
    // === Components ===
    [Header("Components")]
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform Tr;
    [HideInInspector] public SpriteRenderer spr;
    //[HideInInspector] public Animator anim;

    // === Movement ===
    [Header("Movement")]
    float speed = 5;
    float dashSpeed = 10f;
    float dashDuration = 0.4f;
    float dashCooldown = 5f;
    float lastDashTime = -10f;

    float currentSpeed;
    Vector2 lastMoveDir;
    private Vector2 currentInputDir;


    // === State ===
    [HideInInspector] public bool isInvulnerable;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool isKnockedBack;
    bool isDashing = false;


    // === Jumping ===
    [Header("Jumping")]
    public int jump = 7;
    private bool isGrounded;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public Transform groundCheckPoint;

    // === Animations ===
    //[Header("Animations")]

    // === Scripts ===
    [Header("Scripts")]
    BaseMask mask;

    // === Level Map ===
    private LevelMap map;
    private Vector2 facingDir = Vector2.right;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();
        //anim = GetComponent<Animator>();

        currentSpeed = speed;

        map = LevelMap.Instance;

        StartCoroutine(Blink());
        ApplyMask(GameSession.SelectedMask);

    }

    void ApplyMask(MaskType type)
    {
        switch (type)
        {
            case MaskType.Mage:
                mask = gameObject.AddComponent<MageMask>();
                break;

            case MaskType.Melee:
                //mask = gameObject.AddComponent<MeleeMask>();
                break;

            case MaskType.Ranged:
                //mask = gameObject.AddComponent<RangedMask>();
                break;
        }
    }


    void Update()
    {
        if (isKnockedBack) return;

        CheckGrounded();

        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttacks();
        HandleDrop();

        /*if (anim != null)
        {
            anim.SetBool("isGrounded", isGrounded);
        }*/
    }

    void HandleMovement()
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

            // Track facing direction
            facingDir = new Vector2(
                Mathf.RoundToInt(isoX),
                Mathf.RoundToInt(isoY)
            );

            // Check ledge - auto drop
            if (map != null && map.IsAtLedge(transform.position, facingDir))
            {
                transform.position = map.GetPosBelow(transform.position);
                return;
            }
        }

        rb.linearVelocity = new Vector3(
            move.x * speed,
            move.y * speed
        );

        if (move.x != 0)
            spr.flipX = move.x < 0;

        /*if (anim != null)
            anim.SetFloat("Speed", move.sqrMagnitude);*/
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (map != null && map.CanJumpUp(transform.position))
            {
                transform.position = map.GetPosAbove(transform.position);
            }
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time < lastDashTime + dashCooldown) return;
            lastDashTime = Time.time;

            if (map == null) return;

            Vector3? target = map.CanDashAcross(transform.position, facingDir);

            if (target.HasValue)
            {
                // Can dash - gap is 3 or less
                transform.position = target.Value;
            }
            else
            {
                // Can't dash - drop down
                Vector3Int grid = map.GetGrid(transform.position);
                if (grid.z > 0)
                {
                    transform.position = map.GetPosBelow(transform.position);
                }
            }
        }
    }

    void HandleDrop()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (map == null) return;

            Vector3Int grid = map.GetGrid(transform.position);
            if (grid.z > 0)
            {
                transform.position = map.GetPosBelow(transform.position);
            }
        }
    }

    private void HandleAttacks()
    {
        if (!canAttack || mask == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            mask.CastPrimary();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            mask.CastSecondary();
        }
    }

    private void CheckGrounded()
    {
        // Raycast downward to detect ground
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        isGrounded = hit.collider != null;
    }


    // === Coroutines ===
    IEnumerator Dash()
    {
        isDashing = true;
        isInvulnerable = true;

        Vector2 dashDir = lastMoveDir;
        if (dashDir.sqrMagnitude < 0.1f)
        {
            dashDir = spr.flipX ? Vector2.left : Vector2.right;
        }
        dashDir = dashDir.normalized;

        // Apply dash velocity (preserve some Y for mid-air dashes)
        rb.linearVelocity = new Vector2(dashDir.x * dashSpeed, rb.linearVelocity.y * 0.5f);

        yield return new WaitForSeconds(dashDuration);

        isInvulnerable = false;
        isDashing = false;
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


}
