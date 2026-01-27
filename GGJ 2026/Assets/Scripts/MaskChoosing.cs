using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskChoosing : MonoBehaviour
{
    public void SelectMage()
    {
        GameSession.SelectedMask = MaskType.Mage;
        SceneManager.LoadScene("Level1");
    }
    public void SelectRanged()
    {
        GameSession.SelectedMask = MaskType.Ranged;
        SceneManager.LoadScene("Level1");
    }
    public void SelectMelee()
    {
        GameSession.SelectedMask = MaskType.Melee;
        SceneManager.LoadScene("Level1");
    }
}
