using UnityEngine;
using UnityEngine.UI // to talk to slider component

public class Boss : Enemies
{

    // variable to hold the reference to the ui slider
    protected Slider healthbar;

    public override void Start()
    {
        base.Start() // to run normal enemy math first

        //1. find the health bar object by name
        // so u dont drag it everytimne
        GameObject uiObj = GameObject.Find("BossHealthBar");

        if (uiOBJ != null)
        {
            healthBar = uiObj.GetComponent<Slider>();
            // 2. set max value to match the boss total health
            healthBar.maxValue = currentHealth;

            // 3. set current value to full
            healthBar.value = currentHealth;
            
            // 4. make sure the bar is visible 
            healthbar.gameObject.SetActive(true);

        }
        else 
        {
            Debug.LogError("Boss Error: Could not find a slider named 'BossHealthBar' in the scene.");
        }
    }

    // this runs whenever the boss gets hit
    public override void TakeDamage(float amount, float knockbackForce = 0f, Vector2 knockbackDir = default)
    {
        // 1. let the base enemy script handle the actual damage and physics
        base.TakeDamage(amount, knockbackForce, knockbackDir);

        // 2. update the health bar visual
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    public override void Die()
    {
        // hide the bar so it doesn't stay on screen after the boss dies
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
        
        base.Die();
    }

    // placeholder: ramses will fill this in
    public override void Attack() { 
        
    }
}