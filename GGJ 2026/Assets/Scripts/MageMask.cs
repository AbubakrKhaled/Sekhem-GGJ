using UnityEngine;

public class MageMask : BaseMask
{
    Player player;

    [Header("Level 1 - Earth")] 
    // drag the stone pillar prefab here
    public GameObject stonePillarPrefab;
    
    // how wide the circle of pillars will be
    public float pillarRadius = 2.0f; 
    // how many pillars spawn in the ring (6 makes a nice hexagon shape)
    public int pillarCount = 6;

    // --- cooldown settings ---
    // wait 4 seconds between attacks
    public float abilityCooldown = 4f; 
    // keeps track of when we last cast the spell
    private float lastCastTime = -10f; 

    [Header("Secondary Stats")]
    public float earthPushRadius = 2.5f;
    public float earthPushForce = 8f;
    public float earthPushDamage = 5f;

    protected override void Start()
    {
        base.Start();
        // find the player script so we know where to spawn things
        player = GetComponent<Player>();
        
        // auto-fix the level if it defaults to 0
        if(maskLevel == 0) maskLevel = 1; 
    }

    public override void CastPrimary()
    {
        // 1. check cooldown
        // if current time is less than (last time + 4 seconds), we must wait
        if (Time.time < lastCastTime + abilityCooldown)
        {
            Debug.Log("mage mask on cooldown! wait...");
            return;
        }

        // 2. set cooldown timer
        // save the time right now so we can't fire again instantly
        lastCastTime = Time.time;

        // 3. cast spell
        switch (maskLevel)
        {
            case 1: EarthPillars(); break;
            case 2: MudSpike(); break;
            case 3: Fireball(); break;
        }
    }


    protected override void OnMaskUpgraded()
    {
        Debug.Log("mage upgraded to level " + maskLevel);
    }

    // --- spells ---
    void EarthPillars()
    {
        // prevent errors if the prefab is missing
        if (stonePillarPrefab == null) return;

        // start at the player's position
        Vector2 startPos = transform.position;

        // --- circular math logic ---
        // divide 360 degrees by the number of pillars to get even spacing
        // e.g. 360 / 6 = 60 degrees between each pillar
        float angleStep = 360f / pillarCount;

        for (int i = 0; i < pillarCount; i++)
        {
            // 1. calculate the angle for this specific pillar
            float angle = i * angleStep;
            
            // 2. convert degrees to radians (math needs radians for sin/cos)
            float radian = angle * Mathf.Deg2Rad;

            // 3. calculate x and y offset using trigonometry
            float x = Mathf.Cos(radian) * pillarRadius;
            float y = Mathf.Sin(radian) * pillarRadius;

            // 4. set the final spawn position relative to the player
            Vector2 spawnPos = startPos + new Vector2(x, y);

            // create the pillar
            Instantiate(stonePillarPrefab, spawnPos, Quaternion.identity);
        }
        
        Debug.Log("earth pillars cast in a circle!");
    }

    void EarthPush()
    {
        // find all colliders inside the blast radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, earthPushRadius);

        foreach (var hit in hits)
        {
            // check if the object is an enemy
            Enemies enemy = hit.GetComponent<Enemies>();
            if (enemy == null) continue;

            // deal damage
            enemy.TakeDamage(earthPushDamage);

            // apply physics knockback
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // calculate direction from player to enemy
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                
                // stop their movement first so the push is strong
                rb.linearVelocity = Vector2.zero; 
                
                // push them away
                rb.AddForce(dir * earthPushForce, ForceMode2D.Impulse);
            }
        }
    }

    // draws a yellow circle in the editor so we can see the range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, earthPushRadius);
    }

    void MudSpike() { Debug.Log("mud spike"); }
    void MudPool() { Debug.Log("mud pool"); }
    void Fireball() { Debug.Log("fireball"); }
    void FlameBurst() { Debug.Log("flame burst"); }
}