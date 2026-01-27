using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] Slider healthSlider;

    Health playerHealth;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>().GetComponent<Health>();

        healthSlider.value = 1f;

        playerHealth.OnHealthChanged += UpdateHealthBar;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    void UpdateHealthBar(int current, int max)
    {
        healthSlider.value = (float)current / max;
    }
}
