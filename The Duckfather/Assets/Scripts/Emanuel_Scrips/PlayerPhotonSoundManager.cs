using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerPhotonSoundManager : MonoBehaviour
{
    public AudioSource footstepSource;

    public AudioClip footstepSFX;

    public AudioSource gunShootSource;
    public AudioClip[] allGunShootSFX;

    public AudioSource GrabSource;
    public AudioClip[] GrabSFX;

    public AudioSource HurtSource;
    public AudioClip[] allHurtSFX;

    public AudioSource PauseSource;
    public AudioClip PauseSFX;

    public AudioSource UnPauseSource;
    public AudioClip UnPauseSFX;

    public void PlayFootStepSFX()
    {
       GetComponent<PhotonView>().RPC("PlayFootstepSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFootstepSFX_RPC()
    {
        footstepSource.clip = footstepSFX;
        footstepSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        footstepSource.volume = UnityEngine.Random.Range(0.75f, 0.85f);

        footstepSource.Play();
    }

    public void PlayShootSFX(int index)
    {
        GetComponent<PhotonView>().RPC("PlayShootSFX_RPC", RpcTarget.All, index);
    }

    [PunRPC]
    public void PlayShootSFX_RPC(int index)
    {
        gunShootSource.clip = allGunShootSFX[index];

        if(index == 4)
        {
            gunShootSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            gunShootSource.volume = UnityEngine.Random.Range(0.23f, 0.32f);
        }


        gunShootSource.Play();
    }

    public void PlayGrabSFX(int index)
    {
        GetComponent<PhotonView>().RPC("PlayGrabSFX_RPC", RpcTarget.All, index);
    }

    [PunRPC]
    public void PlayGrabSFX_RPC(int index)
    {
        
        if(index == 2)
        {
            GrabSource.clip = GrabSFX[index];
            
        }
        else
        {
            GrabSource.clip = GrabSFX[index];
            
        }
        GrabSource.Play();

    }


    public void PlayHurtSFX()
    {
        GetComponent<PhotonView>().RPC("PlayHurtSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayHurtSFX_RPC(int index)
    {
        HurtSource.clip = allHurtSFX[index];
        HurtSource.Play();
    }

    public void PlayPauseSFX()
    {
        GetComponent<PhotonView>().RPC("PlayPauseSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayPauseSFX_RPC()
    {
        PauseSource.clip = PauseSFX;
        PauseSource.Play();
    }
    public void PlayUnPauseSFX()
    {
        GetComponent<PhotonView>().RPC("PlayUnPauseSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayUnPauseSFX_RPC()
    {
        UnPauseSource.clip = UnPauseSFX;
        UnPauseSource.Play();
    }

}
