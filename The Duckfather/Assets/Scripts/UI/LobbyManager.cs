using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField roomName;
    [SerializeField] GameObject roomPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] TMP_Text roomNameText;
    
    [SerializeField] RoomItem roomItem;
    List<RoomItem> roomItemList = new List<RoomItem>();
    [SerializeField] Transform roomListContent;
    
    [SerializeField] float roomListUpdateInterval = 1.5f;
    float roomListUpdateTimer;
    
    
    List<PlayerItem> playerList = new List<PlayerItem>();
    [SerializeField] PlayerItem playerName;
    [SerializeField] Transform playerListContent;
    
    
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.JoinLobby();
    }
    
    // crate a room with the room name
    
    public void OnClickCreateRoom()
    {
        if(roomName.text.Length >= 1)
        {
               RoomOptions options = new RoomOptions();
                options.MaxPlayers = 8;
                PhotonNetwork.CreateRoom(roomName.text, options);
        }
    }
    
    // join a room with the room name
    public override void OnJoinedRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }
    
    // Update room list
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time > roomListUpdateTimer)
        {
            roomListUpdateTimer = Time.time + roomListUpdateInterval;
            UpdateRoomList(roomList);
        }
        UpdateRoomList(roomList);
    }
    
    void UpdateRoomList(List<RoomInfo> List)
    {
        foreach (RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }
        roomItemList.Clear();
        foreach (RoomInfo room in List)
        {
            RoomItem newItem = Instantiate(roomItem, roomListContent);
            newItem.SetRoonName(room.Name);
            roomItemList.Add(newItem);
        }
    }
    
    // Join a room with the room name
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    
    // Leave the room
    
    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    public override void OnLeftRoom()
    {
        roomPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }
    
    // connect to lobby
    // ReSharper disable Unity.PerformanceAnalysis
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    
    // connect to photon server using the player name
    public override void OnJoinedLobby()
    {
        UpdatePlayerList();
    }
    
  
    
    // connect to photon server using the player name
    
    void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerList)
        {
            Destroy(item.gameObject);
        }
        playerList.Clear();
        
        if(PhotonNetwork.CurrentRoom == null)
            return;
        
        foreach (KeyValuePair<int,Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newItem = Instantiate(playerName, playerListContent);
            newItem.SetPlayerInfo(player.Value);
            playerList.Add(newItem);
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }
    
    public void OnClickOnline()
    {
        PhotonNetwork.ConnectUsingSettings();
        SceneManager.LoadScene("Online");
    }
    
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
