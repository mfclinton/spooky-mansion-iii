using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spawnable
{
    public GameObject prefab;
    public float spawnProbability;
}

[System.Serializable]
public class SpawnAmount
{
    public int numberToSpawn;
    public float spawnProbability;
}

public class Spawner : MonoBehaviour
{
    public Transform shelfSpawnLocations; // The parent transform with 4 children
    public List<Spawnable> spawnables; // The prefabs you want to spawn
    public List<SpawnAmount> spawnAmounts; // The possible amounts to spawn with their probabilities

    private List<Transform> spawnPoints; 

    private void Awake()
    {
        spawnPoints = new List<Transform>();

        // Add all children to the spawnPoints list
        foreach(Transform t in shelfSpawnLocations)
        {
            spawnPoints.Add(t);
        }

        // Shuffle the spawn points
        int n = spawnPoints.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Transform value = spawnPoints[k];
            spawnPoints[k] = spawnPoints[n];
            spawnPoints[n] = value;
        }
    }

    private void Start()
    {
        // Spawn a number of objects determined by the spawn probabilities
        int numberToSpawn = ChooseNumberToSpawn();
        SpawnObjects(numberToSpawn);
    }

    void SpawnObjects(int number)
    {
        for (int i = 0; i < number; i++)
        {
            // Choose prefab to spawn based on spawn probability
            GameObject chosenPrefab = ChoosePrefab();

            // Instantiate prefab at spawn location with the location's rotation
            Instantiate(chosenPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
        }
    }

    GameObject ChoosePrefab()
    {
        // Calculate total spawn probability
        float totalProbability = 0f;
        foreach (Spawnable spawnable in spawnables)
        {
            totalProbability += spawnable.spawnProbability;
        }

        // Generate random spawn value
        float spawnValue = Random.value * totalProbability;

        // Choose prefab based on spawn value
        foreach (Spawnable spawnable in spawnables)
        {
            if (spawnValue < spawnable.spawnProbability)
            {
                return spawnable.prefab;
            }
            spawnValue -= spawnable.spawnProbability;
        }
        return null; // This should never happen if spawn probabilities are setup correctly
    }

    int ChooseNumberToSpawn()
    {
        // Calculate total spawn amount probability
        float totalProbability = 0f;
        foreach (SpawnAmount spawnAmount in spawnAmounts)
        {
            totalProbability += spawnAmount.spawnProbability;
        }

        // Generate random spawn amount value
        float spawnValue = Random.value * totalProbability;

        // Choose spawn amount based on spawn value
        foreach (SpawnAmount spawnAmount in spawnAmounts)
        {
            if (spawnValue < spawnAmount.spawnProbability)
            {
                return spawnAmount.numberToSpawn;
            }
            spawnValue -= spawnAmount.spawnProbability;
        }
        return 0; // This should never happen if spawn probabilities are setup correctly
    }
}
