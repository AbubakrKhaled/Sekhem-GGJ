using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // === Components ===
    [Header("Components")]
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform Tr;
    [HideInInspector] public SpriteRenderer spr;
    [HideInInspector] public Animator Playeranim;

    // === Movement ===
    [Header("Movement")]
    public int speed = 5;
    [HideInInspector] public Vector3 startpos;
    float dashSpeed = 10f;
    float dashDuration = 0.2f;
    float dashCooldown = 5f;
    float lastDashTime;
    float currentSpeed;

    // === Jumping ===
    [Header("Jumping")]
    public int jump = 7;
    private bool isGrounded;

    // === Animations ===
    //[Header("Animations")]
    bool isDashing = false;

    void Start()
    {
        gameObject.tag = "MainPlayer";

        rb = GetComponent<Rigidbody2D>();
        Tr = GetComponent<Transform>();
        spr = GetComponent<SpriteRenderer>();
        Playeranim = GetComponent<Animator>();

        // Position
        startpos = Tr.position;
        currentSpeed = speed;

        StartCoroutine(Blink());  
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleDash();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");

        if (x != 0)
        {
            spr.flipX = x < 0;
        }

        rb.linearVelocity = new Vector2(x * currentSpeed, rb.linearVelocity.y);
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


    // === Collisions ===
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }


    // === Coroutines ===
    IEnumerator Dash()
    {
        isDashing = true;
        currentSpeed = dashSpeed;
        
        yield return new WaitForSeconds(dashDuration);

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
