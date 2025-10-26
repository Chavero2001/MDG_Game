using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class EndScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Text timePlayedText;
    [SerializeField] private TMP_Text facesObtainedText;
    [SerializeField] private TMP_Text enemiesDestroyedText;
    [SerializeField] private TMP_Text wavesSurvivedText;

    [SerializeField] private float countUpDuration = 1.0f; // animate counters

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found. Make sure stats are provided before loading End scene.");
            // fallback: show zeros or try to fetch saved values
            SetAllDisplays(0f, 0, 0,0);
            return;
        }

        // Stop counting time (if not already)
        GameManager.Instance.EndRun();

        float time = GameManager.Instance.TimePlayed;
        int enemies = GameManager.Instance.EnemiesDestroyed;
        float waves = GameManager.Instance.WavesSurvived;
        int faces = GameManager.Instance.FacesObtained;

        // Start animated display
        StartCoroutine(AnimateTime(time));
        StartCoroutine(AnimateIntCounter(faces, facesObtainedText, "Faces Obtained: "));
        StartCoroutine(AnimateIntCounter((int)waves, wavesSurvivedText, "Waves Survived: "));
        StartCoroutine(AnimateIntCounter(enemies, enemiesDestroyedText, "Enemies Destroyed: "));
    }

    private void SetAllDisplays(float time, int faces, int waves, int enemies)
    {
        timePlayedText.text = "Time Played: " + FormatTime(time);
        facesObtainedText.text = "Faces Obtained: " + faces;
        wavesSurvivedText.text = "Waves Survived: " + waves;
        enemiesDestroyedText.text = "Enemies Destroyed: " + enemies;
    }

    private System.Collections.IEnumerator AnimateTime(float targetTime)
    {
        float elapsed = 0f;
        while (elapsed < countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / countUpDuration);
            float displayTime = Mathf.Lerp(0f, targetTime, t);
            timePlayedText.text = "Time Played: " + FormatTime(displayTime);
            yield return null;
        }
        timePlayedText.text = "Time Played: " + FormatTime(targetTime);
    }

    private System.Collections.IEnumerator AnimateIntCounter(int target, TMP_Text textField, string prefix)
    {
        float elapsed = 0f;
        while (elapsed < countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / countUpDuration);
            int display = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
            textField.text = prefix + display;
            yield return null;
        }
        textField.text = prefix + target;
    }

    private string FormatTime(float seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        // Format mm:ss.ff — minutes:seconds:hundredths
        return string.Format("{0:D2}:{1:D2}.{2:D2}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
    }

    // Hook these to buttons
    /*public void OnPlayAgain()
    {
        SpawnWaves.wave = 1;
        // Reset as desired
        GameManager.Instance.ResetRun();
        SceneManager.LoadScene("GameScene"); // replace with your gameplay scene name
    }

    public void OnMainMenu()
    {
        SpawnWaves.wave = 1;
        GameManager.Instance.ResetRun();
        SceneManager.LoadScene("MainMenu");
    }*/
}
