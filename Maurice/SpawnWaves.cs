using System.Collections;
using UnityEngine;

public class SpawnWaves : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;   // Must have tag "Enemy"
    [SerializeField] private int enemiesPerWave = 10;
    [SerializeField] private float timeBetweenSpawns = 0.05f; // small delay so they don't overlap exactly

    [Header("Spawn Points (size = 4)")]
    [SerializeField] private Transform[] spawnPoints = new Transform[4];

    [Header("Tracking")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float checkInterval = 0.5f; // don't check every frame

    private bool isSpawning = false;
    private float checkTimer = 0f;

    private void Start()
    {
        // Initial wave
        StartCoroutine(SpawnWave());
    }

    private void Update()
    {
        // Throttled tag checks for performance
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval && !isSpawning)
        {
            checkTimer = 0f;

            // Count current enemies by tag
            int alive = GameObject.FindGameObjectsWithTag(enemyTag).Length;

            // If all were cleared, start next wave
            if (alive == 0)
            {
                StartCoroutine(SpawnWave());
            }
        }
    }

    private IEnumerator SpawnWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[SpawnWaves] Enemy Prefab is not assigned.");
            yield break;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnWaves] Spawn points are not assigned.");
            yield break;
        }

        isSpawning = true;

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (p != null)
            {
                Instantiate(enemyPrefab, p.position, p.rotation);
            }

            if (timeBetweenSpawns > 0f)
                yield return new WaitForSeconds(timeBetweenSpawns);
            else
                yield return null; // next frame
        }

        isSpawning = false;
    }
}
