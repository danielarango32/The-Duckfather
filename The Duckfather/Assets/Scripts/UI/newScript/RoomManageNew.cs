using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class RoomManagerNew : MonoBehaviourPunCallbacks
{
    public const string MenuSceneName = "Online 2";

    [SerializeField]private int sceneNumber;
    public static RoomManagerNew instance;

    private bool isLeavingMatch;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return; 
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == sceneNumber)
        {
            PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity);
            Debug.Log("PlayerManager instantiated");
        }
    }

    /// <summary>
    /// Salida unica de una partida: destruye los objetos de red del jugador local,
    /// abandona la sala y vuelve al menu cuando Photon confirma la salida.
    /// No desconecta a proposito: seguir en el Master Server es lo que permite
    /// volver a crear partida nada mas llegar al menu.
    /// </summary>
    public static void ExitMatch()
    {
        if (instance != null)
        {
            instance.LeaveMatch();
            return;
        }

        // Sin RoomManagerNew (por ejemplo arrancando la escena de juego
        // directamente desde el editor) solo queda volver al menu.
        Cursor.lockState = CursorLockMode.None;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        SceneManager.LoadScene(MenuSceneName);
    }

    private void LeaveMatch()
    {
        if (isLeavingMatch)
        {
            return;
        }
        isLeavingMatch = true;
        Cursor.lockState = CursorLockMode.None;

        if (!PhotonNetwork.InRoom)
        {
            ReturnToMenu();
            return;
        }

        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Salir de una sala desde el lobby lo gestiona Launcher; aqui solo
        // respondemos a la salida de partida que hemos iniciado nosotros.
        if (!isLeavingMatch)
        {
            return;
        }
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        isLeavingMatch = false;
        SceneManager.LoadScene(MenuSceneName);
    }
}
