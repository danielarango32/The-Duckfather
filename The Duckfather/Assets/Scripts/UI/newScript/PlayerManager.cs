using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    
    PhotonView PV;
    
    GameObject controller;

    private int Kills;
    private int Death;
    
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
        
    }
    
    void CreateController()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate("Pato rolo", spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
        
        Death++;
        
        Hashtable hash = new Hashtable();
        hash.Add("Kills", Death);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    
    public void GetKill()
    {
        PV.RPC(nameof(this.RPC_GetKill), PV.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        Kills++;
        
        Hashtable hash = new Hashtable();
        hash.Add("Kills", Kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    /* public static PlayerManager Find(Player player)
     {
         return FindObjectOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
     }*/
}
