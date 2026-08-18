using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerPhotonSoundManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip footstepSFX;
    [Space]
    public AudioSource jumpSource;
    public AudioClip jumpSFX;
    [Space]
    public AudioSource gunShootSource;
    public AudioClip[] allGunShootSFX;
    [Space]
    public AudioSource GrabSource;
    public AudioClip[] GrabSFX;
    [Space]
    public AudioSource HurtSource;
    public AudioClip[] allHurtSFX;
    [Space]
    public AudioSource PauseSource;
    public AudioClip PauseSFX;
    [Space]
    public AudioSource UnPauseSource;
    public AudioClip UnPauseSFX;
    [Space]
    public AudioSource fallSource;
    public AudioClip fallSFX;
    [Space]
    public AudioSource power1Source;
    public AudioClip power1SFX;
    [Space]
    public AudioSource power2Source;
    public AudioClip power2SFX;
    [Space]
    public AudioSource quackSource;
    public AudioClip[] quackSFX;
    [Space]
    public AudioSource dieSource;
    public AudioClip dieSFX;



    public void PlayFootStepSFX()
    {
       GetComponent<PhotonView>().RPC("PlayFootstepSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFootstepSFX_RPC()
    {
        footstepSource.clip = footstepSFX;
        footstepSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        footstepSource.volume = UnityEngine.Random.Range(0.3f, 0.45f);

        footstepSource.Play();
    }

    public void PlayJumpSFX()
    {
        GetComponent<PhotonView>().RPC("PlayJumpSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayJumpSFX_RPC()
    {
        jumpSource.clip = jumpSFX;
        jumpSource.volume = UnityEngine.Random.Range(0.1f, 0.2f);
        jumpSource.Play();
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
    public void PlayHurtSFX_RPC()
    {
        HurtSource.clip = allHurtSFX[Random.Range(0,allHurtSFX.Length)];
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

    public void PlayFallSFX()
    {
        GetComponent<PhotonView>().RPC("PlayFallSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayFallSFX_RPC()
    {
        fallSource.clip = fallSFX;
        fallSource.volume = UnityEngine.Random.Range(0.23f, 0.32f);
        fallSource.Play();
    }

    public void PlayPower1SFX()
    {
        GetComponent<PhotonView>().RPC("PlayPower1SFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayPower1SFX_RPC()
    {
        power1Source.clip = power1SFX;
        power1Source.Play();
    }
    public void PlayPower2SFX()
    {
        GetComponent<PhotonView>().RPC("PlayPower2SFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayPower2SFX_RPC()
    {
        power2Source.clip = power2SFX;
        power2Source.Play();
    }

    public void PlayQuackSFX()
    {
        GetComponent<PhotonView>().RPC("PlayQuackSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayQuackSFX_RPC()
    {
        quackSource.clip = quackSFX[Random.Range(0,quackSFX.Length)];
        quackSource.Play();
    }
    public void PlayDieSFX()
    {
        GetComponent<PhotonView>().RPC("PlayDieSFX_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlayDieSFX_RPC()
    {
        dieSource.clip = dieSFX;
        dieSource.Play();
    }
}
