using UnityEngine;
using UnityEngine.SceneManagement;

public class MageMask : BaseMask
{
    [Header("Prefabs")]
    public GameObject stonePillarPrefab;     // Level 1: Earth
    public GameObject waterProjectilePrefab; // Level 2: Water
    public GameObject fireProjectilePrefab;  // Level 3: Fire

    [Header("Element Cooldowns")]
    private float earthCooldown = 4f;    // Stone Pillar
    private float waterCooldown = 2.5f;  // Water projectile
    private float fireCooldown = 4f;     // Fire radial burst

    [Header("Element Damage")]
    private float earthDamage = 10f;     // Base damage for Earth
    private float waterDamage = 15f;     // Higher damage for Water
    private float fireDamage = 20f;      // Highest damage for Fire
    
    // Tracks which spell is currently selected (1=earth, 2=water, 3=fire)
    private int selectedElement = 1; 
    private float lastCastTime = -10f;

    protected override void Start()
    {
        base.Start();
        if(maskLevel == 0) maskLevel = 1;

        // If loading into a later level, auto-equip the newest unlocked element
        if (maskLevel >= 2) selectedElement = maskLevel;
    }

    private void Update()
    {
        // Element switching: Q or Right Click
        // Only allow switching if we have unlocked multiple elements
        if (maskLevel >= 2)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
            {
                SwitchElement();
            }
        }
    }

    void SwitchElement()
    {
        selectedElement++;

        // Loop back to 1 if we exceed our current unlocked level
        if (selectedElement > maskLevel)
        {
            selectedElement = 1;
        }

        string[] elementNames = { "", "Earth", "Water", "Fire" };
        Debug.Log($"🔮 Switched to {elementNames[selectedElement]} element!");
    }

    public override void CastPrimary()
    {
        // Check cooldown for CURRENT selected element
        float cooldown = GetCurrentCooldown();
        
        if (Time.time < lastCastTime + cooldown)
        {
            return; // Still on cooldown
        }

        lastCastTime = Time.time;

        // Cast the currently selected element
        switch (selectedElement)
        {
            case 1: CastEarthPillars(); break;
            case 2: CastWaterProjectile(); break;
            case 3: CastFireRadialBurst(); break;
            default: CastEarthPillars(); break;
        }
    }

    float GetCurrentCooldown()
    {
        switch (selectedElement)
        {
            case 1: return earthCooldown;
            case 2: return waterCooldown;
            case 3: return fireCooldown;
            default: return earthCooldown;
        }
    }

    float GetCurrentDamage()
    {
        switch (selectedElement)
        {
            case 1: return earthDamage;
            case 2: return waterDamage;
            case 3: return fireDamage;
            default: return earthDamage;
        }
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("mage upgraded! new element unlocked & stats buffed!");

        // 1. Reduce all cooldowns by 0.5s
        earthCooldown = Mathf.Max(1f, earthCooldown - 0.5f);
        waterCooldown = Mathf.Max(1f, waterCooldown - 0.5f);
        fireCooldown = Mathf.Max(2f, fireCooldown - 0.5f);

        // 2. Increase all damage by +5
        earthDamage += 5f;
        waterDamage += 5f;
        fireDamage += 5f;

        // 3. Auto-equip the newly unlocked element
        selectedElement = maskLevel;
        if (selectedElement > 3) selectedElement = 3;
    }

    // ======= ABILITY IMPLEMENTATIONS =======

    /// <summary>
    /// EARTH (Level 1): Spawn stone pillars in a circle around player
    /// Cooldown: 4s | Damage: 10 (base)
    /// </summary>
    void CastEarthPillars()
    {
        if (stonePillarPrefab == null)
        {
            Debug.LogWarning("Stone Pillar prefab not assigned!");
            return;
        }
        
        // Number of pillars scales with mask level
        int count = 6 + maskLevel; 
        float radius = 2f;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            
            GameObject pillar = Instantiate(stonePillarPrefab, spawnPos, Quaternion.identity);
            
            // Pass damage to pillar if it has the script
            StonePillar pillarScript = pillar.GetComponent<StonePillar>();
            if (pillarScript != null)
            {
                pillarScript.damage = (int)GetCurrentDamage();
            }
        }
        
        Debug.Log($"🪨 Cast Earth Pillars! ({count} pillars, {GetCurrentDamage()} damage each)");
    }

    /// <summary>
    /// WATER (Level 2): Fire water projectile toward mouse
    /// Cooldown: 2.5s | Damage: 15 (higher than Earth)
    /// </summary>
    void CastWaterProjectile()
    {
        if (waterProjectilePrefab == null)
        {
            Debug.LogWarning("Water Projectile prefab not assigned!");
            return;
        }

        // Get mouse direction
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        
        // Spawn water projectile
        GameObject waterObj = Instantiate(waterProjectilePrefab, transform.position, Quaternion.identity);
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        waterObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set up projectile (needs WaterProjectile script)
        WaterProjectile waterScript = waterObj.GetComponent<WaterProjectile>();
        if (waterScript != null)
        {
            waterScript.Setup(direction, 12f, (int)GetCurrentDamage(), 5f);
        }

        Debug.Log($"💧 Cast Water Projectile! ({GetCurrentDamage()} damage)");
    }

    /// <summary>
    /// FIRE (Level 3): Radiate fire projectiles in all directions around player
    /// Cooldown: 4s | Damage: 20 (highest)
    /// </summary>
    void CastFireRadialBurst()
    {
        if (fireProjectilePrefab == null)
        {
            Debug.LogWarning("Fire Projectile prefab not assigned!");
            return;
        }

        // Fire projectiles in a circle (8-12 projectiles based on level)
        int projectileCount = 8 + (maskLevel * 2);
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            // Spawn fire projectile slightly offset from player
            Vector2 spawnPos = (Vector2)transform.position + direction * 0.5f;
            GameObject fireObj = Instantiate(fireProjectilePrefab, spawnPos, Quaternion.identity);
            
            // Rotate to face outward
            float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            fireObj.transform.rotation = Quaternion.Euler(0, 0, rotAngle);

            // Set up projectile (needs FireProjectile script)
            FireProjectile fireScript = fireObj.GetComponent<FireProjectile>();
            if (fireScript != null)
            {
                fireScript.Setup(direction, 10f, (int)GetCurrentDamage(), 6f);
            }
        }
        
        Debug.Log($"🔥 Cast Fire Radial Burst! ({projectileCount} projectiles, {GetCurrentDamage()} damage each)");
    }
}