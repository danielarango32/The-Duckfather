using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Botonaudio : MonoBehaviour
{
    [SerializeField]    AudioSource audioSource;
    [SerializeField]    AudioSource audioSource1;
    [SerializeField]    AudioSource audioSource2;
    [SerializeField]    AudioSource audioSource3;
    [SerializeField]    AudioClip Hoversound;
    [SerializeField]    AudioClip Clicksound;
    
    // play audio hover
    
    public void PlayAudioHover()
    {
        audioSource.PlayOneShot(Hoversound);
        // play 1 time
        
    }
    
    // play audio click
    
    public void PlayAudioClick()
    {
        audioSource.PlayOneShot(Clicksound);
        
    }
    
    
}
