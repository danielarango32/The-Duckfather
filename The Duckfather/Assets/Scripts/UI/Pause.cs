using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public ShootinController shootinController;
    public GameObject _camara;
    
    public bool isPaused;
    
    [SerializeField] GameObject pausa;
    
    public PhotonView PV;
    private PlayerPhotonSoundManager playerPhotonSoundManager;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        if (!PV.IsMine)
        {
            Destroy(pausa);
        }
        playerPhotonSoundManager = GetComponent<PlayerPhotonSoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused == true)
            {
                ClosePauseMenu();
            }

            else {
                OpenPauseMenu();
            }

        }
    }
    
    // open the pause menu whit esc key
    
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        playerMovement.enabled = false;
        mouseLook.enabled = false;
        shootinController.enabled = false;
        //_camara.SetActive(false);
        //Time.timeScale = 0;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Pause");

        playerPhotonSoundManager.PlayPauseSFX();
    }
    
    // close the pause menu
    
    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        playerMovement.enabled = true;
        mouseLook.enabled = true;
        shootinController.enabled = true;
        //_camara.SetActive(true);
        //Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Unpause");
        playerPhotonSoundManager.PlayUnPauseSFX();
    }
    
    // back to lobby
    
    public void BackToLobby()
    {
        ClosePauseMenu();
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.Destroy(GameObject.Find("PlayerManager"));
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
