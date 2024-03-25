using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField playerName;
    [SerializeField] TMP_Text ButtonText;
    
    // connect to photon server using the player name
    public void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnClickConnect()
    {
       if(playerName.text.Length >= 1)
       {
           PhotonNetwork.NickName = playerName.text;
           ButtonText.text = "Connecting...";
           PhotonNetwork.ConnectUsingSettings();
           SceneManager.LoadScene("Lobby");
       }
    }
    
}
