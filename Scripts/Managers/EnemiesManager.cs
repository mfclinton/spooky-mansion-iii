using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private AIEnemy AIEnemyPrefab; // Prefab for the AI Enemy
    [SerializeField] private Transform[] spawnLocations; // List of spawn locations
    [SerializeField] private int numberOfEnemiesToSpawn; // Number of AI Enemies to spawn
    List<AIEnemy> enemies;

    [Header("Blackout Settings")]
    [SerializeField] private float minTimeToBlackOut; // Minimum time to Blackout
    [SerializeField] private float maxTimeToBlackOut; // Maximum time to Blackout

    AdvancedLightController lightController;

    private void Awake()
    {
        lightController = FindObjectOfType<AdvancedLightController>();

        enemies = new List<AIEnemy>();
    }

    private void Start()
    {
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
            SpawnEnemy();

        ScheduleNextBlackout();
    }

    private void SpawnEnemy()
    {
        if (spawnLocations.Length > 0)
        {
            // Choose a random spawn location
            int spawnIndex = Random.Range(0, spawnLocations.Length);
            Transform spawnLocation = spawnLocations[spawnIndex];

            // Instantiate the enemy at the spawn location
            AIEnemy enemy = Instantiate(AIEnemyPrefab, spawnLocation.position, spawnLocation.rotation);
            enemy.OnDeath += () => { OnEnemyDeath(enemy); };
            enemies.Add(enemy);
        }
    }

    private void OnEnemyDeath(AIEnemy enemy) {
        enemies.Remove(enemy);
        SpawnEnemy();
    }

    public void TeleportEnemy(AIEnemy enemy)
    {
        if (spawnLocations.Length > 0)
        {
            int spawnIndex = Random.Range(0, spawnLocations.Length);
            Transform spawnLocation = spawnLocations[spawnIndex];

            enemy.aiAgent.agent.Warp(spawnLocation.position);
        }
    }


    private void ScheduleNextBlackout()
    {
        StartCoroutine(CauseBlackout());
    }

    // Method stub for the CauseBlackout method. To be implemented later.
    private IEnumerator CauseBlackout()
    {
        float delay = Random.Range(minTimeToBlackOut, maxTimeToBlackOut);
        yield return new WaitForSeconds(delay);

        // Turn off all the lights
        lightController.StartBlackout();

        yield return new WaitForSeconds(1f);

        foreach (AIEnemy enemy in enemies)
            TeleportEnemy(enemy);

        // Schedule the next blackout
        ScheduleNextBlackout();
    }
}
