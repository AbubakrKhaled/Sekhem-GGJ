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
        Vector3 spawnPos = transform.position + new Vector3(spawnOffset * dirX, 0, 0);

        if (arrowPrefab != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

            ArrowProjectile arrowScript = arrowObj.GetComponent<ArrowProjectile>();
            if (arrowScript != null)
            {
                // pass the current upgraded stats to the arrow
                arrowScript.Setup(dirX, arrowSpeed, baseDamage, knockbackForce);
            }

            lastFireTime = Time.time;
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