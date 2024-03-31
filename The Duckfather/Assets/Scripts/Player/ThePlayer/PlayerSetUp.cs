using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetUp : MonoBehaviour
{
    public PlayerMovement _playerMove;
    public MouseLook _mouseLook;
    public AimManager _aimManager;
    public AnimatorController _animatorController;
    public ShootinController _shootinController;
    
    [Space]
    public GameObject _camara;
    

    public void IsLocalPlayer()
    {
        _playerMove.enabled = true;
        _mouseLook.enabled = true;

        _camara.SetActive(true);
        _aimManager.enabled = true;

        _animatorController.enabled = true;
        _shootinController.enabled = true;

    }
}
