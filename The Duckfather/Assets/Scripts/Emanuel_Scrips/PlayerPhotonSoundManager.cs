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
    public AudioClip GrabSFX;

    public AudioSource HurtSource;
    public AudioClip[] allHurtSFX;

    public void PlayFootStepSFX()
    {
       GetComponent<PhotonView>().RPC("PlayFootstepSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFootstepSFX_RPC()
    {
        footstepSource.clip = footstepSFX;
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
        gunShootSource.Play();
    }

    public void PlayGrabSFX()
    {
        GetComponent<PhotonView>().RPC("PlayGrabSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayGrabSFX_RPC()
    {
        GrabSource.clip = GrabSFX;
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

}
