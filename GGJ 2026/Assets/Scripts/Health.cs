using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;

    // Current HP (protected so child classes can read/write)
    private int currentHealth;

    // Event triggered whenever health changes <current, max>
    public event Action<int, int> OnHealthChanged;

    // Event triggered when health reaches zero
    public event Action OnDied;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void ApplyDamage(int damage)
    {
        currentHealth -= damage;         // Subtract damage from current health

        currentHealth = Mathf.Max(currentHealth, 0);        // Minimum of 0

        OnHealthChanged?.Invoke(currentHealth, maxHealth);         // Notify any listeners (UI, VFX, audio, etc.)

        if (currentHealth == 0)
        {
            OnDied?.Invoke();
        }
    }

    public virtual void ApplyHeal(int amount)
    {
        currentHealth += amount;         // Add healing

        currentHealth = Mathf.Min(currentHealth, maxHealth);        // Maximum health

        OnHealthChanged?.Invoke(currentHealth, maxHealth);         // Notify listeners

    }

    public virtual void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Notify listeners
    }

    // Getters
    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public void Initialize(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth; // Reset to full health
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Notify listeners
    }
}



