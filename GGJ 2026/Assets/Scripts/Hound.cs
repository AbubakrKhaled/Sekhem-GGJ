using UnityEngine;

public class Hound : Mob
{
    public override void Start()
    {
        // define the hound stats before intitiating level math
        // this overrides whatever is in the inspector
        speed = 6f; // default is 3
        baseHealth = 150f; // tanker than default 100
        base.Start(); // now run math with these numbers
    }
    public override void Attack()
    {
    if (player != null)
        {
        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        
            if (hp != null)
            {
                // Calculate the direction the hit came from
                Vector2 hitDirection = (player.position - transform.position).normalized;

                // Send the damage (as an int) AND the direction
                hp.TakeDamage((int)currentDamage, hitDirection);
                
                Debug.Log("Hound bit player for " + currentDamage);
                }
        }
    }

}