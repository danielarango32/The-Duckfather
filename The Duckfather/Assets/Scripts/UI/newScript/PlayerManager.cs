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
    
    [SerializeField] private int deathsTarget = 5;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject looseUI;
    GameObject controller;

    private int Kills;
    private int Death = 0;
    private PlayerPhotonSoundManager playerPhotonSoundManager;

    SpawnManager spawnManager;

    Pause pause;
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
        
        playerPhotonSoundManager= GetComponent<PlayerPhotonSoundManager>();
    }

    public void Update()
    {
        WinningConditions();
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
        CreateController();
        Death++;
        //playerPhotonSoundManager.PlayDieSFX();
        Hashtable hash = new Hashtable();
        hash.Add("deaths", Death);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("Death: " + Death);
    }

    public void WinningConditions()
    {
         if (PV.IsMine && Death == 5)
         {
             StartCoroutine(Loose());
             Debug.Log("You Lose");
         }
         if( PhotonNetwork.PlayerList.Length == 1 && PV.IsMine)
         {
             StartCoroutine(Wining());
             Debug.Log("You Win");
         }
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

    IEnumerator Wining()
    {
        yield return new WaitForSeconds(2);
        winUI.SetActive(true);
        yield return new WaitForSeconds(2);
        EndGame();
    
        
    }
    IEnumerator Loose()
    {
        yield return new WaitForSeconds(2);
        looseUI.SetActive(true);
        yield return new WaitForSeconds(2);
        EndGame();
        
    }

    void EndGame(){
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.Destroy(GameObject.Find("PlayerManager"));
        PhotonNetwork.Destroy(GameObject.Find("ScoreBoardItem"));
        PhotonNetwork.Destroy(GameObject.Find("Player"));
        PhotonNetwork.Destroy(GameObject.Find("launcher"));
        PhotonNetwork.Destroy(GameObject.Find("MenuManager"));
        PhotonNetwork.Destroy(GameObject.Find("PlayerNameManager"));
        PhotonNetwork.DestroyAll();
        PhotonNetwork.Destroy(GameObject.Find("RoomManager"));
        PhotonNetwork.Destroy(GameObject.Find("Room"));
        PhotonNetwork.Destroy(GameObject.Find("RoomManagerNew"));
        PhotonNetwork.Destroy(GameObject.Find("Menu"));
        PhotonNetwork.Destroy(GameObject.Find("Main"));
        PhotonNetwork.Destroy(GameObject.Find("Room Manager"));


        PhotonNetwork.Destroy(GameObject.Find("launcher"));
        PhotonView.Destroy(PV);
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Online 2");
        //GameObject.FindGameObjectsWithTag("join game screen").SetActive(true);
        //SceneManager.LoadScene("Online 2");
        Cursor.lockState = CursorLockMode.None;
    }
   
}
