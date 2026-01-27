using UnityEngine;

public class MageMask : BaseMask
{
    Player player;

    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
    }

    public override void CastPrimary()
    {
        switch (maskLevel)
        {
            case 1: StonePillars(); break;
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

    // --- spells ---
    void StonePillars() { Debug.Log("Stone Pillars"); }
    void EarthPush() { Debug.Log("Earth Push"); }
    void MudSpike() { Debug.Log("Mud Spike"); }
    void MudPool() { Debug.Log("Mud Pool"); }
    void Fireball() { Debug.Log("Fireball"); }
    void FlameBurst() { Debug.Log("Flame Burst"); }
}
