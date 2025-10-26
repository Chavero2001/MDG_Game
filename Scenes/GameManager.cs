using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float TimePlayed { get; private set; } = 0f;
    public int EnemiesDestroyed { get; private set; } = 0;
    public int WavesSurvived { get; private set; } = 0;
    public int FacesObtained { get; private set; } = 0;
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
        TimePlayed = 0f;
        EnemiesDestroyed = 0;
        WavesSurvived = 0;
        IsRunning = true;
    }
}
