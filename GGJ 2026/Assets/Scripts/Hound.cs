using UnityEngine;

public class Hound : Mob
{
    public override void Start()
    {
        // 1. override the inspector speed settings
        // keeping this exactly as you requested
        speed = 4f; 
        
        // 2. set health specifically for level 1
        // 15 damage (pillar) x 4 hits = 60 health
        baseHealth = 60f; 
        
        // 3. now run the base math using these new numbers
        base.Start(); 
    }

    public override void Attack()
    {
        // check if we have a valid target
        if (player != null)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            
            if (hp != null)
            {
                // 1. calculate direction
                // finding the vector from the hound to the player
                Vector2 hitDirection = (player.position - transform.position).normalized;

                // 2. send damage and direction
                // this ensures the player gets knocked back the right way
                hp.TakeDamage((int)currentDamage, hitDirection);
                
                Debug.Log("hound bit player for " + currentDamage);
            }
        }
    }
}