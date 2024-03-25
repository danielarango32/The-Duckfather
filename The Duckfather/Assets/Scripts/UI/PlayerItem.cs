using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerItem : MonoBehaviour
{
    public TMP_Text playerName;
    
    // set player info
    public void SetPlayerInfo(Player player)
    {
        playerName.text = player.NickName;
    }
}
