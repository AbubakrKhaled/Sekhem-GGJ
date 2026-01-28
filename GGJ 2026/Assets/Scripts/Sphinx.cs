using UnityEngine;
using System.Collections; // needed for the laser timer

public class Sphinx : Mob
{
    [Header("Visuals")]
    // where the laser shoots from (the mouth)
    public Transform mouthPosition;   
    // drag your aseprite laser prefab here
    public GameObject laserPrefab; 
    // how long the laser stays on screen
    public float laserDuration = 0.2f; 

    [Header("Targeting")]
    // crucial: select "player" and "default" (walls) in the inspector
    // this prevents the sphinx from accidentally hitting itself
    public LayerMask whatToHit; 

    public override void Start()
    {
        // define sphinx specific stats
        speed = 0f;          // sphinx is a statue, it never moves
        baseHealth = 200f;   // it has high health (tank)
        attackRange = 10f;   // it can shoot from far away
        
        // run the math in enemies.cs to scale stats based on current level
        base.Start(); 
    }

    public override void Update()
    {
        // safety check: stop if player is dead
        if (player == null) return;
        
        // run the standard mob logic (flipping sprite, attack cooldowns)
        base.Update(); 
    }

    // we use 'new' to hide the original movement code
    // this ensures the sphinx stays perfectly still
    public new void MoveToPlayer() 
    { 
        // do nothing
    }

    public override void Attack()
    {
        // 1. calculate the direction towards the player
        Vector2 direction = (player.position - mouthPosition.position).normalized;

        if (anim != null)
        {
            anim.SetTrigger("attack");
        }

        // 2. fire the invisible math ray (hitscan)
        // this checks "did we hit anything?" instantly using the layer mask
        RaycastHit2D hit = Physics2D.Raycast(mouthPosition.position, direction, attackRange, whatToHit);

        // this variable will remember where the laser needs to end
        Vector2 endPoint;

        if (hit.collider != null)
        {
            // we hit something (wall or player)
            endPoint = hit.point;
            if (Audiomanager.Instance != null)
                Audiomanager.Instance.PlaySFX(Audiomanager.Instance.sphinx);


            // --- damage logic ---
            // check if the thing we hit is the player
            if (hit.collider.CompareTag("Player"))
            {
                // try to find the health script
                PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    // deal damage using the level-scaled damage value
                    hp.TakeDamage((int)currentDamage, direction);
                    Debug.Log("sphinx laser zapped player for " + currentDamage);

                }
            }
        }
        else
        {
            // we missed everything, so shoot laser into the empty distance
            endPoint = (Vector2)mouthPosition.position + (direction * attackRange);
        }

        // 3. draw the stretchy sprite to visualize the shot
        StartCoroutine(SpawnLaserSprite(endPoint));
    }

    // this routine handles the visual effects
    private IEnumerator SpawnLaserSprite(Vector2 targetPosition)
    {
        if (laserPrefab != null)
        {
            // a. spawn the laser at the mouth
            GameObject laser = Instantiate(laserPrefab, mouthPosition.position, Quaternion.identity);

            // --- isometric fix ---
            // force the laser to draw on top of everything (player, floor, walls)
            SpriteRenderer sr = laser.GetComponent<SpriteRenderer>();
            if (sr != null) 
            {
                // 1000 is a very high number, ensuring it renders on top
                sr.sortingOrder = 1000; 
            }

            // b. rotate laser to face the target
            Vector2 dir = targetPosition - (Vector2)mouthPosition.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            laser.transform.rotation = Quaternion.Euler(0, 0, angle);

            // c. stretch the laser to reach the target
            float distance = Vector2.Distance(mouthPosition.position, targetPosition);
            
            // important: this assumes your sprite pivot is set to 'left'!
            // we only stretch the x-axis (width)
            laser.transform.localScale = new Vector3(distance, laser.transform.localScale.y, 1);

            // d. wait for a split second and then delete the laser
            yield return new WaitForSeconds(laserDuration);
            Destroy(laser);
        }
    }
}