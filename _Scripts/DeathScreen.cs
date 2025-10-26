using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameObject Player;
    public static bool IsDeath;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        IsDeath = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDeath ==  true)
        {
            GameManager.Instance.EndRun();
            SceneManager.LoadScene(2);
        }
    }
}
