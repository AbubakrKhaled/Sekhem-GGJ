using UnityEngine;

public class RangerMask : BaseMask
{
    [Header("Ranger Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Combat Stats")]
    // these scale with level
    [SerializeField] private float fireCooldown = 1.0f;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private int baseDamage = 8;
    [SerializeField] private float knockbackForce = 3f;

    private float lastFireTime = -10f;
    private SpriteRenderer playerSpr;

    protected override void Start()
    {
        base.Start();
        playerSpr = GetComponent<SpriteRenderer>();
    }

    public override void CastPrimary()
    {
        if (Time.time < lastFireTime + fireCooldown) return;

        float dirX = (playerSpr != null && playerSpr.flipX) ? -1f : 1f;
        Vector3 baseSpawnPos = transform.position + new Vector3(spawnOffset * dirX, 0, 0);

        if (arrowPrefab != null)
        {
            // Determine number of arrows and spread based on mask level
            int arrowCount = maskLevel;  // Level 1 = 1 arrow, Level 2 = 2 arrows, Level 3 = 3 arrows
            float spreadAngle = 0f;
            
            if (maskLevel == 2)
            {
                spreadAngle = 15f;  // 15 degrees spread for 2 arrows
            }
            else if (maskLevel >= 3)
            {
                spreadAngle = 20f;  // 20 degrees spread for 3 arrows
            }

            // Fire arrows in spread pattern
            for (int i = 0; i < arrowCount; i++)
            {
                // Calculate angle offset for this arrow
                float angleOffset = 0f;
                
                if (arrowCount == 1)
                {
                    // Single arrow: straight ahead
                    angleOffset = 0f;
                }
                else if (arrowCount == 2)
                {
                    // Two arrows: one at +spread/2, one at -spread/2
                    angleOffset = (i == 0) ? spreadAngle / 2f : -spreadAngle / 2f;
                }
                else if (arrowCount == 3)
                {
                    // Three arrows: center, +spread, -spread
                    if (i == 0) angleOffset = 0f;           // Center
                    else if (i == 1) angleOffset = spreadAngle;  // Up
                    else angleOffset = -spreadAngle;             // Down
                }

                // Calculate direction with spread
                float angleRad = angleOffset * Mathf.Deg2Rad;
                float dirY = Mathf.Sin(angleRad);
                Vector2 direction = new Vector2(dirX, dirY).normalized;


                // Trigger player animation
                Player player = GetComponent<Player>();
                if (player != null && player.anim != null)
                {
                    player.anim.SetTrigger("attack");
                }

                // Spawn arrow
                GameObject arrowObj = Instantiate(arrowPrefab, baseSpawnPos, Quaternion.identity);

                ArrowProjectile arrowScript = arrowObj.GetComponent<ArrowProjectile>();
                if (arrowScript != null)
                {
                    // Pass direction as a 2D vector for spread shot
                    arrowScript.Setup(direction.x, arrowSpeed, baseDamage, knockbackForce, direction.y);
                }
            }

            lastFireTime = Time.time;
            
            if (arrowCount > 1)
            {
                Debug.Log($"Ranger fired {arrowCount} arrows in spread pattern!");
            }
        }
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("ranger mask stats scaled up!");

        // 1. shoot faster
        fireCooldown = Mathf.Max(0.3f, fireCooldown - 0.2f);

        // 2. hit harder
        baseDamage += 5;

        // 3. faster arrows
        arrowSpeed += 2f;
    }
}