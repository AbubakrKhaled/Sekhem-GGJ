using UnityEngine;
using System.Collections.Generic; // needed to use lists

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    // drag empty game objects here to tell enemies where to appear
    public Transform[] spawnPoints; 
    
    // how fast enemies spawn (in seconds)
    public float timeBetweenSpawns = 2f;
    
    // drag your mummy, hound, and sphinx prefabs here
    // we use 'mob' type because that is where the spawnweight variable is
    public List<Mob> enemyPrefabs; 

    private float spawnTimer;

    void Update()
    {
        // count up the timer
        spawnTimer += Time.deltaTime;

        // if timer is done
        if (spawnTimer >= timeBetweenSpawns)
        {
            SpawnRandomEnemy();
            spawnTimer = 0f; // reset timer
        }
    }

    void SpawnRandomEnemy()
    {
        // safety check: if lists are empty, stop to avoid errors
        if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0) return;

        // 1. pick a random location from the list
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 2. calculate the total "tickets" in the lottery
        // example: mummy(50) + hound(25) + sphinx(5) = 80 total tickets
        int totalWeight = 0;
        foreach (Mob m in enemyPrefabs)
        {
            totalWeight += m.spawnWeight;
            Debug.Log($"[SPAWNER] {m.enemyName} has weight {m.spawnWeight}");
        }

        Debug.Log($"[SPAWNER] Total weight pool: {totalWeight}");

        // 3. pick a winning ticket number
        // picks a number between 0 and 80
        int randomValue = Random.Range(0, totalWeight);
        Debug.Log($"[SPAWNER] Random value picked: {randomValue}");

        // 4. find out who owns that winning ticket
        int cumulativeWeight = 0;
        foreach (Mob m in enemyPrefabs)
        {
            cumulativeWeight += m.spawnWeight;
            
            // check if the random number falls inside this enemy's range
            if (randomValue < cumulativeWeight)
            {
                // we found the winner! spawn it
                Debug.Log($"[SPAWNER] ✅ SPAWNING: {m.enemyName}");
                GameObject enemy = Instantiate(m.gameObject, randomPoint.position, Quaternion.identity);
                
                // FORCE VISIBILITY FIX
                SpriteRenderer spr = enemy.GetComponent<SpriteRenderer>();
                if (spr != null)
                {
                    spr.sortingOrder = 5; // Force visible layer
                    spr.sortingLayerName = "Default";
                }
                
                // FORCE SCALE FIX
                enemy.transform.localScale = new Vector3(3, 3, 1); // Bigger than 1
                
                return; // exit the function
            }
        }
        
        Debug.LogWarning("[SPAWNER] ⚠️ Failed to spawn any enemy! Check weights.");
    }
}