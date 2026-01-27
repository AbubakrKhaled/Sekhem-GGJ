using UnityEngine;

public class Mummy : Mob
{
    public override void Start()
    {
        base.Start(); // <--- FIXED: Added semicolon here

        // handles if you forgot to set name in inspector to mummy
        if (string.IsNullOrEmpty(enemyName)) enemyName = "Mummy";
    }

    public override void Attack()
    {
        // checks if the player is still alive and has health
        if (player != null)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            
            if (hp != null)
            {
                // 1. Calculate the direction for knockback
                Vector2 direction = (player.position - transform.position).normalized;

                // 2. Convert float damage to int and send direction
                hp.TakeDamage((int)currentDamage, direction);
                
                Debug.Log("Mummy hit player!");
            }
        }  
    }
}