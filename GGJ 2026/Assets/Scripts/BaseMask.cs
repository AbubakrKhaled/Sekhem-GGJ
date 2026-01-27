using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public abstract class BaseMask : MonoBehaviour
{
    public int maskLevel = 1;

    public int fragmentsCollected = 0;
    public int fragmentsRequired = 4;


    protected virtual void Start()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 2:
                fragmentsRequired = 3;
                break;

            case 3:
                fragmentsRequired = 6;
                break;

            case 4:
                fragmentsRequired = 9;
                break;
        }
    }

    public virtual void CollectFragment()
    {
        fragmentsCollected++;

        if (fragmentsCollected >= fragmentsRequired)
        {
            UpgradeMask();
        }
    }

    protected virtual void UpgradeMask()
    {
        maskLevel++;
        OnMaskUpgraded();
    }

    protected abstract void OnMaskUpgraded();


    public abstract void CastPrimary();
    
}
