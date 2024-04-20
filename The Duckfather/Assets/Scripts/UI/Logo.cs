using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class Logo : MonoBehaviour
{
    public GameObject logoDF;
    public GameObject Sutdio;

  
    void Start()
    {
        StartCoroutine(LogoTime());
    }
    
    IEnumerator LogoTime()
    {
        yield return new WaitForSeconds(4);
        logoDF.SetActive(true);
        Sutdio.SetActive(false);
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Menu");
    }
    
}
