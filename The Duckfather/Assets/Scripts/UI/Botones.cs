using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class MenuBotones : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text ButtonText;
    [SerializeField] string scene1;
    
    // change scene using button
    
    public void OnClickScee()
    {
        PhotonNetwork.ConnectUsingSettings();
        SceneManager.LoadScene("crearJugador");
        
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
