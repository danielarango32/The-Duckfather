using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList instance;

    public GameObject RoomManagerGameObject;
    public Roomandlobbymanager roomandlobbymanager;
    
    [Header("ui")] 
    public Transform roomParentList;
    public GameObject roomListItemPrefab;
    
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    
    public void ChangeRoomToCreatName(string _roomName)
    {
        roomandlobbymanager.roomNameToJoin = _roomName;
    }
    
    
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        
        PhotonNetwork.ConnectUsingSettings();
    }
    
    
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (cachedRoomList.Count <= 0)
        {
            cachedRoomList = roomList;
        }
        else
        {
            foreach (var room in roomList)
            {
                for (int i = 0; i < cachedRoomList.Count; i++)
                {

                    if (cachedRoomList[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = cachedRoomList;

                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }
                        
                        cachedRoomList = newList;
                    }
                }
            }   
        }
        
        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform roomItem  in roomParentList)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomParentList);

            roomItem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = room.PlayerCount + " /8" ;
            
            roomItem.GetComponent<RoomItemButton>().roomName = room.Name;
        }
    }
    
    public void JoinRoomByName(string _name)
    {
        roomandlobbymanager.roomNameToJoin = _name;
        RoomManagerGameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
