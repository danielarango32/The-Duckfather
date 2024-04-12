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
    [SerializeField]
    private Slider timeSlider;
    
    public TMP_Text timeText;
    
    public float time = 180;
    
    private bool stopTimer = false;
    
    // Start is called before the first frame update
    void Start()
    {
        stopTimer = false;
        timeSlider.maxValue = time;
        timeSlider.value = time;
    }

    // Update is called once per frame
    void Update()
    {
        int timer = (int)PhotonNetwork.CurrentRoom.CustomProperties["Time"]; 
        int minutes = Mathf.FloorToInt((int)PhotonNetwork.CurrentRoom.CustomProperties["Time"] / 60);
        int seconds = Mathf.FloorToInt((int)PhotonNetwork.CurrentRoom.CustomProperties["Time"] % 60);
        String textTimer = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timer <= 0)
        {
            StopTimer();
        }
        if (stopTimer == false)
        {
            timeText.text = textTimer;
            timeSlider.value = time;
        }
        if (timer <= 60)
        {
            this.ChangeColor();
        }
        else if (timer <= 120)
        {
            this.timeSlider.fillRect.GetComponent<Image>().color = Color.yellow;
        }
    }
    
    // slide change color over time
    
    public void ChangeColor()
    {
        timeSlider.fillRect.GetComponent<Image>().color = Color.red;
    }
    
    // stop the timer
    
    public void StopTimer()
    {
        stopTimer = true;
        SceneManager.LoadScene("Online");
        PhotonNetwork.LeaveRoom();
        
    }
    
    
}
