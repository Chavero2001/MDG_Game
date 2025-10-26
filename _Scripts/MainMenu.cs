using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject CreditsPanel;

    public void PlayGame()
    {
        Debug.Log("Moving to next scene");
        SceneManager.LoadScene("Maurice"); // replace with your actual game scene name
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
