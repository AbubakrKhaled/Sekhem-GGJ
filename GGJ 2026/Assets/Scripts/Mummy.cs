using UnityEngine;

public class Mummy : Mob
{
    public override void Start()
    {
        // 1. set health specifically for level 1
        // 15 damage (pillar) x 2 hits = 30 health
        baseHealth = 30f; 

        // 2. run the standard enemy setup (finding player, calculating growth)
        base.Start(); 

        // safety check: gives it a name if we forgot in the inspector
        if (string.IsNullOrEmpty(enemyName)) enemyName = "Mummy";
    }

    public override void Attack()
    {
        // check if player exists before trying to hit them
        if (player != null)
        {
            // try to find the player's health script
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            
            if (hp != null)
            {
                // 1. calculate direction
                // math: (player pos - mummy pos) gives direction towards the player
                Vector2 direction = (player.position - transform.position).normalized;

                // 2. deal damage
                // convert damage to int because player health uses integers
                hp.TakeDamage((int)currentDamage, direction);
                if (anim != null)
                {
                    anim.SetTrigger("attack");
                }


                Debug.Log("mummy hit player!");
                if (Audiomanager.Instance != null)
                    Audiomanager.Instance.PlaySFX(Audiomanager.Instance.mummy);

            }
        }  
    }
}