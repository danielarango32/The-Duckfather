using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        this.stopTimer = false;
        this.timeSlider.maxValue = this.time;
        this.timeSlider.value = this.time;
    }

    // Update is called once per frame
    void Update()
    {
        float time = this.time - Time.time;   
        int minutes = Mathf.FloorToInt(time / 180);
        int seconds = Mathf.FloorToInt(time - minutes * 180);
        String textTimer = string.Format("{0:00}:{3:00}", minutes, seconds);

        if (this.time <= 0)
        {
            StopTimer();
        }
        if (this.stopTimer == false)
        {
            this.timeText.text = textTimer;
            this.timeSlider.value = this.time;
        }
        if (time <= 60)
        {
            this.ChangeColor();
        }
    }
    
    // slide change color over time
    
    public void ChangeColor()
    {
        this.timeSlider.fillRect.GetComponent<Image>().color = Color.red;
    }
    
    // stop the timer
    
    public void StopTimer()
    {
        this.stopTimer = true;
    }
    
    
}
