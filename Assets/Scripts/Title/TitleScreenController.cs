using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string StartScene;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject creditsScreen;

    //As of right now this loads the home scene
    public void StartGame()
    {
        //Uses its own ref to scenemanager because why not
        SceneManager.LoadScene(StartScene);
    }

    //Opens the settings screen
    public void OpenSettingsScreen()
    {
        if(titleScreen.activeInHierarchy)
        {
            titleScreen.SetActive(false);
        }

        settingsScreen.SetActive(true);
    }

    //Goes from settings screen to title screen
    public void CloseSettingsScreen()
    {
        if (settingsScreen.activeInHierarchy)
        {
            settingsScreen.SetActive(false);
        }

        titleScreen.SetActive(true);
    }

    //Opens credits screen
    public void OpenCreditsScreen()
    {
        if (titleScreen.activeInHierarchy)
        {
            titleScreen.SetActive(false);
        }

        creditsScreen.SetActive(true);
    }

    //Goes from credits screen to title screen
    public void CloseCreditsScreen()
    {
        if (creditsScreen.activeInHierarchy)
        {
            creditsScreen.SetActive(false);
        }

        titleScreen.SetActive(true);
    }

    //Closes the application
    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
