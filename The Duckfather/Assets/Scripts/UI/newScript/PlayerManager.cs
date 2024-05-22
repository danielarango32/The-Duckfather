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
    [SerializeField] private string patoName;
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
            RandomSkin();
            CreateController();
            
        }
        
        
    }
    
    void CreateController()
    {           
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(patoName, spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });

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
    
    public void RandomSkin()
    {
        int skinIndex = Random.Range(0, 3);
        if (skinIndex == 0)
        {
            patoName = "Pato 1";
        }
        else if (skinIndex == 1)
        {
            patoName = "Pato 2";
        }
        else if (skinIndex == 2)
        {
            patoName = "Pato 3";
        }
        Debug.Log("Skin: " + patoName);
    }
    /* public static PlayerManager Find(Player player)
     {
         return FindObjectOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
     }*/
}
