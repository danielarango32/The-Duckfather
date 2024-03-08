using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetUp : MonoBehaviour
{
    public PlayerMovement _playerMove;
    public MouseLook _mouseLook;
    
    
    [Space]
    public GameObject _camara;
    

    public void IsLocalPlayer()
    {
        _playerMove.enabled = true;
        _mouseLook.enabled = true;

        _camara.SetActive(true);

    }
}
