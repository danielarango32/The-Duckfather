using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Timer : MonoBehaviour
{
    
    public TMP_Text timeText;
    
    public float time = 180;
    
    
    private bool stopTimer = false;
    
    // Start is called before the first frame update
    void Start()
    {
        stopTimer = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        float timer = time - Time.time;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer - minutes * 60f);
        string textTimer = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        if (timer <= 0)
        {
            StopTimer();
        }
        if (stopTimer == false)
        {
            timeText.text = textTimer;
        }
    }
    
    // stop the timer
    
    public void StopTimer()
    {
        stopTimer = true;
        SceneManager.LoadScene("Online 2");
        PhotonNetwork.LeaveRoom();
        
    }
    
    
}
