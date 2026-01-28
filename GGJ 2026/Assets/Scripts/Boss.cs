using UnityEngine;
using UnityEngine.UI; // <--- ADDED SEMICOLON HERE

public class Boss : Enemies
{
    // variable to hold the reference to the ui slider
    protected Slider healthBar;

    public override void Start()
    {
        base.Start(); 

        // 1. automatically find the health bar object by name
        GameObject uiObj = GameObject.Find("BossHealthBar");
        
        if (uiObj != null)
        {
            healthBar = uiObj.GetComponent<Slider>();
            
            // 2. setup the bar values
            healthBar.maxValue = currentHealth;
            healthBar.value = currentHealth;
            healthBar.gameObject.SetActive(true); 
        }
    }

    public override void TakeDamage(float amount, float knockbackForce = 0f, Vector2 knockbackDir = default)
    {
        base.TakeDamage(amount, knockbackForce, knockbackDir);

        // update the ui slider
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    public override void Die()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
        base.Die();
    }

    public override void Attack() { }
}