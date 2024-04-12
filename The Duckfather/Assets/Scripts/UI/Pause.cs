using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    
    public bool isPaused;
    
    // Start is called before the first frame update
    void Start()
    {
        this.pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }  
    }
    
    // open the pause menu whit esc key
    
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
        Debug.Log("Pause");
    }
    
    // close the pause menu
    
    public void ClosePauseMenu()
    {
        this.pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
        Debug.Log("Unpause");
    }
    
    // back to lobby
    
    public void BackToLobby()
    {
        ClosePauseMenu();
        //GameObject.FindGameObjectsWithTag("join game screen").SetActive(true);
        SceneManager.LoadScene("Online");
        PhotonNetwork.LeaveRoom();
    }
    
}
