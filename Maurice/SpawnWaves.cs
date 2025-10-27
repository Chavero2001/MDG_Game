using System.Collections;
using UnityEngine;

public class SpawnWaves : MonoBehaviour
{
    public AudioSource noise;
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab1;   // Must have tag "Enemy"
    [SerializeField] private GameObject enemyPrefab2;
    [SerializeField] private GameObject enemyPrefab3;
    [SerializeField] private int startingEnemiesWave = 10;
    [SerializeField] private float timeBetweenSpawns = 0.05f; // small delay so they don't overlap exactly

    [Header("Spawn Points (size = 4)")]
    [SerializeField] private Transform[] spawnPoints = new Transform[4];

    [Header("Tracking")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float checkInterval = 0.5f; // don't check every frame

    private bool isSpawning = false;
    private float checkTimer = 0f;
    public static float wave=1f;
    public float waveIncrement = 1f;
    private void Start()
    {
        wave = 1;
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
                wave += waveIncrement;
                StartCoroutine(SpawnWave());
            }
        }
    }

    private IEnumerator SpawnWave()
    {
        if (enemyPrefab1 == null)
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
        noise.Play();
        for (int i = 0; i < startingEnemiesWave* Mathf.Pow(wave,1.3f) ; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (p != null)
            {
                GameObject[] treePrefabs = { enemyPrefab1, enemyPrefab2, enemyPrefab3};
                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                Instantiate(prefab, p.position, p.rotation);
            }

            if (timeBetweenSpawns > 0f)
                yield return new WaitForSeconds(timeBetweenSpawns);
            else
                yield return null; // next frame
        }

        isSpawning = false;
    }
}
