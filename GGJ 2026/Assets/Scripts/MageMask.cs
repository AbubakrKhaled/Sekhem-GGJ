using UnityEngine;
using UnityEngine.SceneManagement;

public class MageMask : BaseMask
{
    [Header("Prefabs")]
    public GameObject stonePillarPrefab;     // Level 1: Earth
    public GameObject waterProjectilePrefab; // Level 2: Water
    public GameObject fireballPrefab;        // Level 3: Fire

    [Header("Dynamic Stats")]
    public float abilityCooldown = 4f; 
    public float damage = 10f;
    
    // tracks which spell is currently selected (1=earth, 2=water, 3=fire)
    private int selectedElement = 1; 
    private float lastCastTime = -10f; 

    protected override void Start()
    {
        base.Start();
        if(maskLevel == 0) maskLevel = 1;

        // if loading into a later level (scene 3 or 4), auto-equip the best element
        if (maskLevel >= 2) selectedElement = 2;
    }

    private void Update()
    {
        // --- weapon switching input ---
        // only allow switching if we have unlocked higher levels
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

        // loop back to 1 if we exceed our current unlocked level
        if (selectedElement > maskLevel)
        {
            selectedElement = 1;
        }

        Debug.Log("switched element to: " + selectedElement);
    }

    public override void CastPrimary()
    {
        if (Time.time < lastCastTime + abilityCooldown) return;

        lastCastTime = Time.time;

        // cast the currently selected element
        switch (selectedElement)
        {
            case 1: CastEarthPillars(); break;
            case 2: CastWaterAttack(); break;
            case 3: CastFireball(); break;
            default: CastEarthPillars(); break;
        }
    }

    protected override void OnMaskUpgraded()
    {
        Debug.Log("mage upgraded! new element unlocked & stats buffed!");

        // 1. buff stats
        abilityCooldown = Mathf.Max(0.5f, abilityCooldown - 0.5f);
        damage += 5f;

        // 2. auto-equip the new element
        selectedElement = maskLevel;
        if (selectedElement > 3) selectedElement = 3;
    }

    // --- spells ---
    void CastEarthPillars()
    {
        if (stonePillarPrefab == null) return;
        
        int count = 6 + maskLevel; 
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
            Instantiate(stonePillarPrefab, spawnPos, Quaternion.identity);
        }
    }

    void CastWaterAttack()
    {
        if (waterProjectilePrefab == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - transform.position).normalized;
        
        GameObject waterObj = Instantiate(waterProjectilePrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        waterObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D rb = waterObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * 12f;
    }

    void CastFireball()
    {
        if (fireballPrefab == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - transform.position).normalized;

        GameObject fb = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = fb.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * 10f;
    }
}