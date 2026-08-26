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

    private int Death = 0;

    SpawnManager spawnManager;

    Pause pause;

    private const int MinimumPlayersForWin = 2;

    // Estado de fin de partida: sin esto, WinningConditions() arrancaba una
    // corrutina nueva en cada frame mientras la condicion siguiera cumpliendose.
    private bool matchEnded;
    private bool matchHasHadRivals;

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

    public void Update()
    {
        WinningConditions();
    }
    
    void CreateController()
    {           
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(patoName, spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });

    }

    // La llama LifeManager.SecuenciaDeMuerte() tras el VFX de muerte y el
    // respawnDelay: el SFX y las particulas de muerte se disparan alla, antes
    // de este punto, mientras el controller viejo seguia en pie.
    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        //PhotonNetwork.Destroy(gameObject);

        // Death++ y SetCustomProperties van ANTES de CreateController(): el
        // pato nuevo lee "deaths" en su propio Start() (DeathsCounterDisplay),
        // asi que si se creara primero, su HUD arrancaria mostrando el
        // conteo de antes de esta muerte.
        Death++;
        Hashtable hash = new Hashtable();
        hash.Add("deaths", Death);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("Death: " + Death);

        CreateController();
    }

    public void WinningConditions()
    {
        if (!PV.IsMine || matchEnded)
        {
            return;
        }

        // La victoria solo cuenta si en algun momento hubo rivales. Al crear
        // una partida estas solo en la sala, y eso disparaba la victoria en el
        // primer frame.
        if (PhotonNetwork.PlayerList.Length >= MinimumPlayersForWin)
        {
            matchHasHadRivals = true;
        }

        if (Death == 5)
        {
            matchEnded = true;
            Debug.Log("You Lose");
            StartCoroutine(Loose());
            return;
        }

        if (matchHasHadRivals && PhotonNetwork.PlayerList.Length == 1)
        {
            matchEnded = true;
            Debug.Log("You Win");
            StartCoroutine(Wining());
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

    // El conteo de kills quedo implementado en LifeManager.AcreditarKill():
    // arranca desde el mismo golpe que mata (donde se sabe PhotonMessageInfo.
    // Sender, el atacante), no desde aqui, que ya no tiene esa informacion.
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

    void EndGame()
    {
        RoomManagerNew.ExitMatch();
    }
   
}
