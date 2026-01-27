using UnityEngine;

public class Ramses : Boss
{
    public override void Start()
    {
        // 1. set health hardcoded for level 1
        // 15 damage (pillar) x 20 hits = 300 health
        baseHealth = 300f; 
        enemyName = "Ramses II";

        // 2. run the boss start (which finds the UI)
        base.Start(); 
    }

    public override void Attack()
    {
        // basic collision attack logic
        if (player != null)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                // calculate pushback direction
                Vector2 dir = (player.position - transform.position).normalized;
                // deal 20 damage on touch
                hp.TakeDamage(20, dir); 
            }
        }
    }
}