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
        Debug.Log("Conectando...");


        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Conexion Existosa");


        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();


        Debug.Log("Conectado a un lobby");
        
        PhotonNetwork.JoinOrCreateRoom("test", null, null);

    }

    public override void OnJoinedRoom()
    {
        base.OnLeftRoom();

        Debug.Log("Conectado a una Sala");

        GameObject _playerP = PhotonNetwork.Instantiate(player.name, Spawn.position, Quaternion.identity);
    }
}
