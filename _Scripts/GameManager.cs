using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float TimePlayed { get; private set; } = 0f;
    public int EnemiesDestroyed { get; private set; } = 0; //To do
    public float WavesSurvived { get; private set; } = 0f;//To do
    public int FacesObtained { get; private set; } = 0;//To do
    public bool IsRunning { get; private set; } = true; //Starts the time played

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        WavesSurvived = SpawnWaves.wave;
        if (IsRunning)
            TimePlayed += Time.deltaTime;
    }

    //Methods to call
    public void AddEnemyDestroyed(int amount = 1) => EnemiesDestroyed += amount;
    public void AddWaveSurvived(int amount = 1) => WavesSurvived += amount;
    public void AddFacesObtained(int amount = 1) => FacesObtained += amount;

    // Call when the run ends
    public void EndRun()
    {
        IsRunning = false;
    }
    // Optional: reset for a new run
    public void ResetRun()
    {
        Debug.Log($"ResetRun called on instance {GetInstanceID()}. Before: time={TimePlayed}, enemies={EnemiesDestroyed}, waves={WavesSurvived}");
        TimePlayed = 0f;
        EnemiesDestroyed = 0;
        WavesSurvived = 0;
        IsRunning = true;
        Debug.Log($"After: time={TimePlayed}, enemies={EnemiesDestroyed}, waves={WavesSurvived}");
    }
}
