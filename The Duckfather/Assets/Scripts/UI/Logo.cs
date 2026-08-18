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
    public GameObject skipButton;
    [SerializeField] private VideoPlayer videoPlayer;


    void Start()
    {
        StartCoroutine(LogoTime());
        videoPlayer.loopPointReached += OnfinishVideoChangeScene;
    }

    public void OnfinishVideoChangeScene(VideoPlayer vp)
    {
        // This is the method that will be called when the video finishes playing
        // You can add your code here to change the scene
        // For example, you can use the following code to change the scene  
        SceneManager.LoadScene("Online 2");
    }

    public void Skip()
    {
        SceneManager.LoadScene("Online 2");
    }
    IEnumerator LogoTime()
    {
        yield return new WaitForSeconds(2);
        logoDF.SetActive(false);
        Sutdio.SetActive(true);
        skipButton.SetActive(true);
        yield break;
    }

    
}
