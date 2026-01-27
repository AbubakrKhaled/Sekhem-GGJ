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
    float dashSpeed = 10f;
    float dashDuration = 0.5f;
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
    private bool isGrounded;

    // === Animations ===
    bool isDashing = false;

    // === Scripts ===
    [Header("Scripts")]
    BaseMask mask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();

        currentSpeed = speed;

        StartCoroutine(Blink());

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
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(x, y);

        if (move.sqrMagnitude > 0.01f)
        {
            lastMoveDir = move.normalized;
        }

        if (move.x != 0)
        {
            spr.flipX = move.x < 0;
        }

        rb.linearVelocity = move * currentSpeed; 
    }

    private void HandleJump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time > lastDashTime + dashCooldown)
            {
                lastDashTime = Time.time;
                StartCoroutine(Dash());
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

    // === Collisions ===
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // === Coroutines ===
    IEnumerator Dash()
    {
        isDashing = true;
        currentSpeed = dashSpeed;
        isInvulnerable = true;

        yield return new WaitForSeconds(dashDuration);

        isInvulnerable = false;
        currentSpeed = speed;
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