using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenMenu : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene(1); //Start directly a new game
    }
    public void ReturnMenu()
    {
        SceneManager.LoadScene(0); //Return to menu
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
