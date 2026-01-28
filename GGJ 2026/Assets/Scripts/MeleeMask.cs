using System.Collections;
using UnityEngine;

public class MeleeMask : BaseMask
{
    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 0.8f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Combat Stats")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private Vector2 hitboxSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private float reachOffset = 1.0f;

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

        // 1. Setup
        float dirX = (playerSpr != null && playerSpr.flipX) ? -1f : 1f;
        Vector3 hitCenter = transform.position + new Vector3(reachOffset * dirX, 0, 0);
        int currentDamage = baseDamage + (maskLevel * 2);
        Vector2 currentSize = hitboxSize;
        float pushForce = 5f;

        if (comboStage == 3)
        {
            currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
            currentSize *= 1.2f;
            pushForce = 10f;
        }

        // 2. Scan for Enemies
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, currentSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Enemies enemyScript = hit.GetComponentInParent<Enemies>();
            if (enemyScript != null)
            {
                Vector2 knockbackDir = new Vector2(dirX, 0);
                enemyScript.TakeDamage(currentDamage, pushForce, knockbackDir);
            }
        }

        // 3. THIS IS THE CRITICAL LINE THAT FIXES THE ERROR
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("Melee Mask Upgraded");
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