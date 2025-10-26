using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject CreditsPanel;
    [SerializeField] GameObject[] UFOs;

    Scene scene;

    void Start()
    {
        scene = SceneManager.GetActiveScene();
        Debug.Log("Active Scene name is: " + scene.name + "\nActive Scene index: " + scene.buildIndex);
    }

    public void PlayGame()
    {
        
        
        Debug.Log("Moving to next scene");
        //GameManager.Instance.
        for (int i = 0; i < UFOs.Length; i++)
        {
            Animator anim = UFOs[i].GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Leaving");
            }
        }
        StartCoroutine(WaitBeforTransitioning());

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

    private IEnumerator WaitBeforTransitioning()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(1); // replace with your actual game scene name
    }
}
