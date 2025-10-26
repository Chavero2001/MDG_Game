using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject CreditsPanel;

    Scene scene;

    void Start()
    {
        scene = SceneManager.GetActiveScene();
        Debug.Log("Active Scene name is: " + scene.name + "\nActive Scene index: " + scene.buildIndex);
    }

    public void PlayGame()
    {
        
        
        Debug.Log("Moving to next scene");
        SceneManager.LoadScene(1); // replace with your actual game scene name
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit(); // does not quit in editor
    }

     public void ShowCredits()
    {
        CreditsPanel.SetActive(true);
    }
    public void HideCredits()
    {
        CreditsPanel.SetActive(false);
    }
}
