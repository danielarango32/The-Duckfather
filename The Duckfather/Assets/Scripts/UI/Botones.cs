using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class MenuBotones : MonoBehaviourPunCallbacks
{
    [SerializeField] string scene1;
    
    // change scene using button
    
    public void OnClickScee()
    {
        SceneManager.LoadScene("Online");
        
    }
    
    
    public void ChangeScene1()
    {
        SceneManager.LoadScene(scene1);
    }
    
   
    
    // exit game
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
