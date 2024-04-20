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
    [Space]
    public GameObject _camara;
    public GameObject _camaraCinemachine;
    public string nickName;
    
    public TextMeshPro nickNameText;
    

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

    }
    
    [PunRPC]
    public void SetNickName(string _name)
    {
        nickName = _name;
        
        nickNameText.text = nickName;
    }
}
