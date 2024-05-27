using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class Botones : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] gameObjectToShow;
    [SerializeField] private GameObject[] gameObjectToHide;
    
    // change scene using button
    
    public void OnClickScee()
    {
        StartCoroutine(OnlineTime());
    }

    IEnumerator OnlineTime()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Online 2");
    }

    public void ShowGameObject()
    {
        StartCoroutine(ShowStuffCoroutine());
        StartCoroutine(HideStuffCoroutine());
    }
    
    IEnumerator ShowStuffCoroutine()
    {
        yield return new WaitForSeconds(1);

        if(gameObjectToShow != null)
        {
            for (int i =0; i < gameObjectToShow.Length; i++)
            {
                gameObjectToShow[i].SetActive(true);
            }
        }
            
        yield break;
           
    }
    IEnumerator HideStuffCoroutine()
    {
        yield return new WaitForSeconds(1);

        if(gameObjectToHide != null)
        {
            for (int i =0; i < gameObjectToHide.Length; i++)
            {
                gameObjectToHide[i].SetActive(false);
            }
        }
            
        yield break;
           
    }
    

   
    
    // exit game
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
