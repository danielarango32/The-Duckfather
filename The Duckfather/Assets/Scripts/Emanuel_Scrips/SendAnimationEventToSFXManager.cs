using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendAnimationEventToSFXManager : MonoBehaviour
{
    public PlayerPhotonSoundManager playerPhotonSoundManager;


    public void TriggerFootstepSFX()
    {
        playerPhotonSoundManager.PlayFootStepSFX();
        
    }

    public void TriggerJumpSFX()
    {
        playerPhotonSoundManager.PlayJumpSFX();

    }
    public void TriggerFallSFX()
    {
        playerPhotonSoundManager.PlayFallSFX();

    }
}
