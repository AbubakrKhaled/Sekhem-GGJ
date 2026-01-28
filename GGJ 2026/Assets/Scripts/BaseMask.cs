using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseMask : MonoBehaviour
{
    public int maskLevel = 1;

    public int fragmentsCollected = 0;
    public int fragmentsRequired = 4;

    protected virtual void Start()
    {
        // automatically adjust difficulty based on which level/scene we are in
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 2: fragmentsRequired = 3; break;
            case 3: fragmentsRequired = 6; break;
            case 4: fragmentsRequired = 9; break;
        }
    }

    public virtual void CollectFragment()
    {
        fragmentsCollected++;
        Debug.Log($"🎯 Fragment collected! ({fragmentsCollected}/{fragmentsRequired})");

        // check if we have enough to trigger the event
        if (fragmentsCollected >= fragmentsRequired)
        {
            Debug.Log($"✅ THRESHOLD REACHED! Upgrading mask...");
            UpgradeMask();
        }
        else
        {
            Debug.Log($"Need {fragmentsRequired - fragmentsCollected} more fragment(s)");
        }
    }

    protected virtual void UpgradeMask()
    {
        // 1. increment level number
        maskLevel++;
        
        // 2. UPGRADE STATS FIRST
        // we call the child class (mage/melee) to buff damage/cooldowns immediately.
        // this code runs and finishes BEFORE the next line starts.
        OnMaskUpgraded();

        // 3. SPAWN BOSS SECOND
        // now that the player is strong, we summon the boss.
        if (BossSpawner.Instance != null)
        {
            BossSpawner.Instance.SpawnBoss();
        }
        else
        {
            Debug.LogWarning("mask upgraded, but no boss spawner found in scene!");
        }
    }

    // abstract methods the child masks must implement
    protected abstract void OnMaskUpgraded();
    public abstract void CastPrimary();
}