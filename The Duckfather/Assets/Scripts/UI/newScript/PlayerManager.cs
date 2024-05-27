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
    public GameObject Camaraloose;
    PhotonView PV;
    
    GameObject controller;

    private int Kills;
    private int Death = 0;
    private PlayerPhotonSoundManager playerPhotonSoundManager;

    SpawnManager spawnManager;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {

        Camaraloose.SetActive(false);
        
        if (PV.IsMine)
        {
            RandomSkin();
            CreateController();
            
        }
        
        playerPhotonSoundManager= GetComponent<PlayerPhotonSoundManager>();
    }
    
    void CreateController()
    {           
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(patoName, spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });

    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        //PhotonNetwork.Destroy(gameObject);
        //CreateController();
        Camaraloose.SetActive(true);
        Death++;
        //playerPhotonSoundManager.PlayDieSFX();
        Hashtable hash = new Hashtable();
        hash.Add("deaths", Death);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("Death: " + Death);
    }

    // if the player dont die and the camera dont loose after all other players die and active ther camara win

    /*public void Win(Hashtable changedProps)
    {
        
        if (Camaraloose.activeSelf == false && changedProps.ContainsKey("deaths")) 
        {
            Debug.Log("You Win");
        }
        else if (Camaraloose.activeSelf == true && changedProps.ContainsKey("deaths"))
        {
            Debug.Log("You Lose");
        }
        
    }*/
    
    /*public void GetKill()
    {
        PV.RPC(nameof(this.RPC_GetKill), PV.Owner);
    }

    [PunRPC]
    void RPC_GetKill()
    {
        Kills++;
        playerPhotonSoundManager.PlayQuackSFX();
        Hashtable hash = new Hashtable();
        hash.Add("Kills", Kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }*/
    
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
   
}
