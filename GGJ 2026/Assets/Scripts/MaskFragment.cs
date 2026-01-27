using UnityEngine;

public class MaskFragment : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseMask mask = other.GetComponent<BaseMask>();
        if (mask == null) return;

        mask.CollectFragment();
        Destroy(gameObject);
    }
}
