using UnityEngine;

// inherits from enemies, so it has access to health, speed, damage, etc
public abstract class Mob : Enemies 
{
    [Header("Spawner Settings")]
    public int spawnWeight = 10;
    // timer to track when we last attacked
    private float lastAttackTime;
    
    // tracks which way the sprite is currently facing
    //private bool isFacingRight = true;

    protected Animator anim;
    protected bool isBack;


    public override void Start()
    {
        // this runs the code in enemies.cs
        // this is what calculates the health/damage based on the current level
        anim = GetComponent<Animator>();

        base.Start(); 
    }

    public virtual void Update()
    {
        // if player is missing, stop everything
        if (player == null) return;
        Vector2 toPlayer = player.position - transform.position;

        // --- FRONT / BACK ---
        isBack = toPlayer.y > 0;

        // --- LEFT / RIGHT ---
        if (Mathf.Abs(toPlayer.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(toPlayer.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // --- ANIMATION ---
        if (anim != null)
        {
            bool moving = Vector2.Distance(transform.position, player.position) > attackRange;
            anim.SetBool("isMoving", moving);
            anim.SetBool("isBack", isBack);
        }


        //// --- sprite flipping logic ---
        //// if player is to the left but we are facing right, flip
        //if (player.position.x < transform.position.x && isFacingRight)
        //{
        //    Flip();
        //}
        //// if player is to the right but we are facing left, flip
        //else if (player.position.x > transform.position.x && !isFacingRight)
        //{
        //    Flip();
        //}

        // --- movement logic ---
        // check distance ignoring z-axis (depth) for 2d
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // we are close enough, check cooldown
            if (Time.time >= lastAttackTime + attackSpeed)
            {
                // call the attack function (defined in mummy/hound/etc)
                Attack(); 
                lastAttackTime = Time.time; // reset timer
            }
        }
        else
        {
            // too far, move towards player
            MoveToPlayer();
        }
    }

    public void MoveToPlayer()
    {
        // move position towards player using the 'speed' variable from enemies.cs
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    // this flips the image horizontally
    //private void Flip()
    //{
    //    isFacingRight = !isFacingRight;
        
    //    // grab the current scale
    //    Vector3 myScale = transform.localScale;
        
    //    // multiply x by -1 to mirror the sprite
    //    myScale.x *= -1;
        
    //    // apply the new scale
    //    transform.localScale = myScale;
    //}
}