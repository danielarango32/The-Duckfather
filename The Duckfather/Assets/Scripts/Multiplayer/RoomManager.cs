using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;

    [Space]
    public Transform Spawn;


    private void Start()
    {
        Debug.Log("Connecting......");


        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected To Server");


        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();


        Debug.Log("We are connected and in a room");
        
        PhotonNetwork.JoinOrCreateRoom("test", null, null);

    }

    public override void OnJoinedRoom()
    {
        base.OnLeftRoom();

        Debug.Log("On a Room");

        GameObject _playerP = PhotonNetwork.Instantiate(player.name, Spawn.position, Quaternion.identity);
    }
}
