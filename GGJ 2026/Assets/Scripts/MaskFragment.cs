using UnityEngine;
using System.Linq;

public class MaskFragment : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Fragment touched by: {other.gameObject.name}");
        
        BaseMask mask = other.GetComponent<BaseMask>();
        
        if (mask == null)
        {
            var components = other.GetComponents<Component>();
            var componentNames = string.Join(", ", components.Select(c => c.GetType().Name));
            Debug.LogWarning($"No BaseMask found on {other.gameObject.name}! Available components: {componentNames}");
            return;
        }

        Debug.Log($"✅ Mask found! Collecting fragment...");
        mask.CollectFragment();
        Destroy(gameObject);
    }
}
