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
        StartCoroutine(OnlineTime());
    }

    IEnumerator OnlineTime()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Online");
    }

    public void ChangeScene1()
    {
        StartCoroutine(creditTime());
    }
    
    IEnumerator creditTime()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(scene1);
    }
    
   
    
    // exit game
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
