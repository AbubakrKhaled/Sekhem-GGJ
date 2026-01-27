using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;
    private Player player;

    [Header("Hit Reaction")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.25f;

    private void Awake()
    {
        health = GetComponent<Health>();
        player = GetComponent<Player>();
    }


    private void OnEnable()
    {
        health.OnDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandlePlayerDeath;
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (player.isInvulnerable) return; // Ignore damage if invulnerable

        health.ApplyDamage(damage);

        StartCoroutine(HitReaction(hitDirection));
    }

    public void Heal(int amount)
    {
        health.ApplyHeal(amount);
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(ReloadScene());
    }

    IEnumerator HitReaction(Vector2 hitDir)
    {
        // Lock player
        player.isInvulnerable = true;
        player.isKnockedBack = true;
        player.canAttack = false;

        // Flash red ONCE
        player.spr.color = Color.red;

        // Apply knockback
        player.rb.linearVelocity = Vector2.zero;
        Vector2 force = new Vector2(hitDir.x, 0.5f).normalized;
        player.rb.AddForce(force * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        // Restore state
        player.rb.linearVelocity = Vector2.zero;
        player.spr.color = Color.white;

        player.isInvulnerable = false;
        player.isKnockedBack = false;
        player.canAttack = true;
    }


    IEnumerator ReloadScene()
    {
        // Optional: death delay / animation
        yield return new WaitForSeconds(0.4f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

  

}

