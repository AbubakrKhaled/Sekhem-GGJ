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
        }

        // 3. pick a winning ticket number
        // picks a number between 0 and 80
        int randomValue = Random.Range(0, totalWeight);

        // 4. find out who owns that winning ticket
        foreach (Mob m in enemyPrefabs)
        {
            // check if the random number falls inside this enemy's range
            if (randomValue < m.spawnWeight)
            {
                // we found the winner! spawn it
                Instantiate(m.gameObject, randomPoint.position, Quaternion.identity);
                return; // exit the function
            }

            // if not, subtract this enemy's weight and check the next one
            // (this effectively moves the "window" to the next ticket holder)
            randomValue -= m.spawnWeight;
        }
    }
}