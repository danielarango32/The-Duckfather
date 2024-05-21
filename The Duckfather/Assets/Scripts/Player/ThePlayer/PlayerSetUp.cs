using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerSetUp : MonoBehaviour
{
    public PlayerMovement _playerMove;
    public MouseLook _mouseLook;
    public AimManager _aimManager;
    public AnimatorController _animatorController;
    public ShootinController _shootinController;
    public LifeManager _lifeManager;
    public PlayerPhotonSoundManager _soundManager;
    [Space]
    public GameObject _camara;
    public GameObject _camaraCinemachine;
    public string nickName;
    
    public TextMeshPro nickNameText;
    
    public PhotonView PV;
    
   private void Awake()
    {
        
    }
    
    private void Start()
    {
        if (!PV.IsMine)
        {
            //Destroy(GetComponentInChildren<Camera>().gameObject);
            IsNotLocalPlayer();
        }
        else
        {
            IsLocalPlayer();
        }
    }

  /* private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }
    }*/


    public void IsLocalPlayer()
    {
        _lifeManager.enabled = true;
        _playerMove.enabled = true;
        _mouseLook.enabled = true;

        _camara.SetActive(true);
        _camaraCinemachine.SetActive(true);
        _aimManager.enabled = true;

        _animatorController.enabled = true;
        _shootinController.enabled = true;

        _soundManager.enabled = true;

    }
    public void IsNotLocalPlayer()
    {
        _lifeManager.enabled = false;
        _playerMove.enabled = false;
        _mouseLook.enabled = false;

        _camara.SetActive(false);
        _camaraCinemachine.SetActive(false);
        _aimManager.enabled = false;

        _animatorController.enabled = false;
        _shootinController.enabled = false;

    }

    
    [PunRPC]
    public void SetNickName(string _name)
    {
        nickName = _name;
        
        nickNameText.text = nickName;
    }
}
