using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    LobbyManager lobbyManager;
    
    // Start is called before the first frame update
    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
    }
    
    // set room name
    public void SetRoonName(string _roomName)
    {
        roomName.text = _roomName;
    }
    
    // join the room
    
    public void OnClickRoom()
    {
        lobbyManager.JoinRoom(roomName.text);
    }
}
