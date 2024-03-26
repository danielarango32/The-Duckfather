using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Roomandlobbymanager : MonoBehaviourPunCallbacks
{
    public static Roomandlobbymanager Instance;
    
    public GameObject player;

    [Space]
    public Transform Spawn;

    [Space] 
    public GameObject roomCam;
    
    [Space]
    public GameObject nameUI;
    
    public GameObject ConnectingUI;
    
    
    private String nickName = "Player";
    
    public string roomNameToJoin = "test";
    private void Awake()
    {
        Instance = this;
    }

    public void ChangeNickName(String _name)
    {
        nickName = _name;
    }
    
    public void JoinRoomButtonPressed()
    {
        
        Debug.Log("Conectando...");
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        
        nameUI.SetActive(false);
        ConnectingUI.SetActive(true);
        
    }
    

    public override void OnJoinedRoom()
    {
        base.OnLeftRoom();

        Debug.Log("Conectado a una Sala");
        
        SpawnPlayer();
    }
    
    public void SpawnPlayer()
    {
        GameObject _player = PhotonNetwork.Instantiate(player.name, Spawn.position, Quaternion.identity);
        _player.GetComponent<PlayerSetUp>().IsLocalPlayer();
        //_playerP.GetComponent<Health>.isLocalPlayer = true;
        
        _player.GetComponent<PhotonView>().RPC("SetNickName", RpcTarget.AllBuffered, nickName);
        
    }
    
}
