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
    [HideInInspector] public Animator anim;

    // === Movement ===
    [Header("Movement")]
    public int speed = 5;
    //[HideInInspector] public Vector3 startpos;
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
    //[Header("Animations")]
    bool isDashing = false;

    // === Scripts ===
    [Header("Scripts")]
    BaseMask mask;


    void Start()
    {
        //gameObject.tag = "MainPlayer";

        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Position
        //startpos = Tr.position;
        currentSpeed = speed;

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
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttacks();

        anim.SetBool("isGrounded", isGrounded);
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(x, y);

        // Store last movement direction for dash & knockback recovery
        if (move.sqrMagnitude > 0.01f)
        {
            lastMoveDir = move.normalized;
        }

        if (move.x != 0)
        {
            spr.flipX = move.x < 0;
        }

        rb.linearVelocity = move * currentSpeed;

        anim.SetFloat("Speed", move.sqrMagnitude);
    }

    private void HandleJump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jump);
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


}
