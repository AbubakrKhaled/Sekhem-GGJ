using System.Collections;
using UnityEngine;

public class MeleeMask : BaseMask
{
    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private float attackDuration = 0.2f;
    
    // this gets faster every level
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Combat Stats")]
    // these increase every level
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private Vector2 hitboxSize = new Vector2(4f, 4f); // EMERGENCY: Was 1.5x1.5, now HUGE
    [SerializeField] private float reachOffset = 2.5f; // EMERGENCY: Was 1.0, now extended reach

    // internal state
    private int currentComboIndex = 0;
    private float lastAttackTime;
    private bool isAttacking;
    private SpriteRenderer playerSpr;
    private Animator playerAnim;

    protected override void Start()
    {
        base.Start();
        playerSpr = GetComponent<SpriteRenderer>();
        playerAnim = GetComponent<Animator>(); // Get Player's animator
    }

    public override void CastPrimary()
    {
        if (isAttacking) return;

        float timeSinceLast = Time.time - lastAttackTime;

        // reset combo if too late
        if (timeSinceLast > comboWindow || currentComboIndex >= 3)
            currentComboIndex = 0;

        // check cooldown
        if (currentComboIndex == 0 && timeSinceLast < attackCooldown) return;

        currentComboIndex++;
        StartCoroutine(PerformAttack(currentComboIndex));
        lastAttackTime = Time.time;
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("melee mask stats scaled up!");

        // 1. increase damage
        baseDamage += 5;

        // 2. increase speed (lower cooldown)
        attackCooldown = Mathf.Max(0.1f, attackCooldown - 0.1f);

        // 3. increase range (bigger hitbox & reach)
        hitboxSize *= 1.15f; // 15% bigger
        reachOffset *= 1.1f; // 10% further
    }

    private IEnumerator PerformAttack(int comboStage)
    {
        isAttacking = true;

        float dirX = (playerSpr != null && playerSpr.flipX) ? -1f : 1f;
        Vector3 hitCenter = transform.position + new Vector3(reachOffset * dirX, 0, 0);

        // calculate final damage
        int currentDamage = baseDamage;
        float pushForce = 5f;

        // Trigger attack animation on Player
        if (playerAnim != null)
        {
            playerAnim.SetTrigger("attack");
        }

        // 3rd hit of combo is always a finisher (stronger)
        if (comboStage == 3)
        {
            currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
            pushForce = 10f;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, hitboxSize, 0f);

        Debug.Log($"[MELEE] Attack at {hitCenter}, found {hits.Length} colliders");

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Debug.Log($"[MELEE] Checking hit on: {hit.gameObject.name}");

            Enemies enemyScript = hit.GetComponentInParent<Enemies>();
            if (enemyScript != null)
            {
                Debug.Log($"[MELEE] ✅ HIT {enemyScript.enemyName} for {currentDamage} damage!");
                Vector2 knockbackDir = new Vector2(dirX, 0);
                enemyScript.TakeDamage(currentDamage, pushForce, knockbackDir);
            }
            else
            {
                Debug.Log($"[MELEE] ❌ No Enemies component found on {hit.gameObject.name}");
            }
        }

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    private void OnDrawGizmos()
    {
        if (playerSpr == null) playerSpr = GetComponent<SpriteRenderer>();
        Gizmos.color = Color.red;
        float dirX = (playerSpr != null && playerSpr.flipX) ? -1f : 1f;
        Vector3 hitCenter = transform.position + new Vector3(reachOffset * dirX, 0, 0);
        Gizmos.DrawWireCube(hitCenter, hitboxSize);
    }
}