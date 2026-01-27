using System.Collections;
using UnityEngine;

public class MeleeMask : BaseMask
{
    [Header("Combo Settings")]
    private float comboWindow = 0.8f;
    private float attackDuration = 0.2f;
    private float attackCooldown = 0.4f;

    [Header("Combat Stats")]
    private int baseDamage = 10;
    private Vector2 hitboxSize = new Vector2(1.5f, 1.5f);
    private float reachOffset = 1.0f;

    // We keep this for reference, but the fix below makes the code work even if this is wrong
    private LayerMask enemyLayer;

    // Internal State
    private int currentComboIndex = 0;
    private float lastAttackTime;
    private bool isAttacking;

    // References
    private Player player;
    private SpriteRenderer playerSpr;

    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
        playerSpr = GetComponent<SpriteRenderer>();
    }

    public override void CastPrimary()
    {
        if (isAttacking) return;

        float timeSinceLast = Time.time - lastAttackTime;

        if (timeSinceLast > comboWindow || currentComboIndex >= 3)
            currentComboIndex = 0;

        if (currentComboIndex == 0 && timeSinceLast < attackCooldown) return;

        currentComboIndex++;
        StartCoroutine(PerformAttack(currentComboIndex));

        lastAttackTime = Time.time;
    }

    private IEnumerator PerformAttack(int comboStage)
    {
        isAttacking = true;

        // 1. Determine Direction
        float dirX = (playerSpr != null && playerSpr.flipX) ? -1f : 1f;

        // 2. Calculate Hitbox Position
        Vector3 center = transform.position;
        Vector3 hitCenter = center + new Vector3(reachOffset * dirX, 0, 0);

        // 3. Logic for Combo Stages
        int currentDamage = baseDamage + (maskLevel * 2);
        Vector2 currentSize = hitboxSize;
        float pushForce = 5f;

        if (comboStage == 3)
        {
            currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
            currentSize *= 1.2f;
            pushForce = 10f;
        }

        // --- THE FIX IS HERE ---
        // Instead of filtering by 'enemyLayer', we check EVERYTHING.
        // This guarantees we find the enemy even if your Layer settings are wrong.
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, currentSize, 0f);

        foreach (Collider2D hit in hits)
        {
            // Don't hit yourself
            if (hit.gameObject == gameObject) continue;

            // Try to find the script on the object or its parent
            Enemies enemyScript = hit.GetComponentInParent<Enemies>();

            if (enemyScript != null)
            {
                Vector2 knockbackDir = new Vector2(dirX, 0);
                enemyScript.TakeDamage(currentDamage, pushForce, knockbackDir);
            }
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("Melee Mask Level Up!");
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