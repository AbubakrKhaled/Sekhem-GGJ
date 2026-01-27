using UnityEngine;

public class MageMask : BaseMask
{
    Player player;

    [Header("Level 1 — Earth")]
    public GameObject stonePillarPrefab;
    public float pillarSpacing = 1.2f;
    public int pillarCount = 3;

    public float earthPushRadius = 2.5f;
    public float earthPushForce = 8f;
    public float earthPushDamage = 5f;



    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
    }

    public override void CastPrimary()
    {
        switch (maskLevel)
        {
            case 1: EarthPillars(); break;
            case 2: MudSpike(); break;
            case 3: Fireball(); break;
        }
    }

    public override void CastSecondary()
    {
        switch (maskLevel)
        {
            case 1: EarthPush(); break;
            case 2: MudPool(); break;
            case 3: FlameBurst(); break;
        }
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("Mage upgraded to level " + maskLevel);

    }

    // --- Spells ---
    void EarthPillars()
    {
        Vector2 dir = player.spr.flipX ? Vector2.left : Vector2.right;
        Vector2 startPos = transform.position;

        for (int i = 1; i <= pillarCount; i++)
        {
            Vector2 spawnPos = startPos + dir * pillarSpacing * i;
            Instantiate(stonePillarPrefab, spawnPos, Quaternion.identity);
        }
    }

    void EarthPush()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            earthPushRadius
        );

        foreach (var hit in hits)
        {
            Enemies enemy = hit.GetComponent<Enemies>();
            if (enemy == null) continue;

            // deal light damage
            enemy.TakeDamage(earthPushDamage);

            // apply knockback if possible
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(dir * earthPushForce, ForceMode2D.Impulse);
            }
        }
    }

    // Debug helper
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, earthPushRadius);
    }
    void MudSpike() { Debug.Log("Mud Spike"); }
    void MudPool() { Debug.Log("Mud Pool"); }
    void Fireball() { Debug.Log("Fireball"); }
    void FlameBurst() { Debug.Log("Flame Burst"); }
}
