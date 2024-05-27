using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OnFinish : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string sceneName;

    public void Start()
    {
        videoPlayer.loopPointReached += OnfinishVideoChangeScene;
        
    }
    public void OnfinishVideoChangeScene(VideoPlayer vp)
    {
        // This is the method that will be called when the video finishes playing
        // You can add your code here to change the scene
        // For example, you can use the following code to change the scene  
        SceneManager.LoadScene(sceneName);
    }
}
