using UnityEngine;

public class RangerMask : BaseMask
{
    [Header("Ranger Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float fireCooldown = 2.5f;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Combat Stats")]
    [SerializeField] private int baseDamage = 8;
    [SerializeField] private float knockbackForce = 3f;

    // Internal State
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

            // Try to set up the arrow, but don't crash if script is missing
            ArrowProjectile arrowScript = arrowObj.GetComponent<ArrowProjectile>();
            if (arrowScript != null)
            {
                int currentDamage = baseDamage + (maskLevel * 2);
                arrowScript.Setup(dirX, arrowSpeed, currentDamage, knockbackForce);
            }

            lastFireTime = Time.time;
        }
        else
        {
            Debug.LogError("RangerMask: No Arrow Prefab assigned in the Inspector!");
        }
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("Ranger Mask Leveled Up!");
    }
}