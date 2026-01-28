using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    // singleton instance allows base mask to find this script easily
    public static BossSpawner Instance;

    public GameObject bossPrefab; // drag ramses prefab here
    public Transform spawnPoint;  // drag an empty gameobject (location) here

    void Awake()
    {
        Instance = this;
    }

    public void SpawnBoss()
    {
        // safety check to prevent crashes
        if (bossPrefab != null && spawnPoint != null)
        {
            // Disable all enemy spawners (Unity 6 safe)
            EnemySpawner[] allSpawners =
                FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

            foreach (EnemySpawner enemySpawner in allSpawners)
            {
                enemySpawner.enabled = false;
            }

            Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("BOSS SPAWNED: " + bossPrefab.name);
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
                spawner.enabled = false;
            if (Audiomanager.Instance != null)
                Audiomanager.Instance.PlayBossMusic();
        }
        else
        {
            Debug.LogError("boss spawner is missing the prefab or spawnpoint!");
        }
    }
}