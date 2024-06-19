using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame(int getStartScene)
    {
        SceneManager.LoadScene(getStartScene);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
