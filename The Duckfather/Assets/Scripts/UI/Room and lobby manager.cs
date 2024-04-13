using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Roomandlobbymanager : MonoBehaviourPunCallbacks
{
    public static Roomandlobbymanager Instance;
    
    public GameObject player;

    [Space]
    public Transform[] Spawns;

    [Space] 
    public GameObject roomCam;
    
    [Space]
    public GameObject nameUI;
    
    public GameObject ConnectingUI;
    
    public GameObject TimerUI;
    
    
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
         
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, new RoomOptions{MaxPlayers = 8}, null);
        
        nameUI.SetActive(false);
        ConnectingUI.SetActive(true);
        
    }
    

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Conectado a una Sala");
        
        
        roomCam.SetActive(false);
        
        TimerUI.SetActive(true);
        
        SpawnPlayer();
    }
    
    public void SpawnPlayer()
    {
        Transform Spawn = Spawns[UnityEngine.Random.Range(0, Spawns.Length)];
            
        GameObject _player = PhotonNetwork.Instantiate(player.name, Spawn.position, Quaternion.identity);
        _player.GetComponent<PlayerSetUp>().IsLocalPlayer();
        _player.GetComponent<LifeManager>().isLocalPlayer = true;
        
        _player.GetComponent<PhotonView>().RPC("SetNickName", RpcTarget.AllBuffered, nickName);
        
    }
    
}
