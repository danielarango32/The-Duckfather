using System;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Timer : MonoBehaviourPunCallbacks
{
    public TMP_Text timeText;
    public float timerDuration = 180f; // Duración del temporizador en segundos
    private float timer;
    private bool timerStarted = false;

    private void Start()
    {
        // Inicializar el temporizador en el MasterClient
        if (PhotonNetwork.IsMasterClient)
        {
            timer = timerDuration;
            timerStarted = true;
            photonView.RPC("SyncTimer", RpcTarget.All, timer, timerStarted);
        }
    }

    private void Awake()
    {
        UpdateTimerText(timerDuration); // Asegurarse de que el texto del temporizador se actualice en Awake
    }

    private void Update()
    {
        if (timerStarted)
        {
            timer -= Time.deltaTime;
            UpdateTimerText(timer);
            Debug.Log("Timer updating for " + PhotonNetwork.NickName + ": " + timer);

            if (timer <= 0f && PhotonNetwork.IsMasterClient)
            {
                // Finalizar la partida y la sesión
                EndGame();
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room: " + newPlayer.NickName);
        // Sincronizar el temporizador con el nuevo jugador
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("MasterClient: Syncing timer with new player " + newPlayer.NickName);
            photonView.RPC("SyncTimer", newPlayer, timer, timerStarted);
        }
    }

    [PunRPC]
    private void SyncTimer(float currentTimer, bool isStarted)
    {
        Debug.Log("SyncTimer RPC called with currentTimer: " + currentTimer + " and isStarted: " + isStarted);
        timer = currentTimer;
        timerStarted = isStarted;
        UpdateTimerText(timer);
    }

    private void EndGame()
    {
        // Finalizar la partida y la sesión
        Debug.Log("Game ended.");
        PhotonNetwork.DestroyAll();
        PhotonNetwork.LeaveRoom();
    }

    private void UpdateTimerText(float currentTimer)
    {
        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer - minutes * 60f);
        string textTimer = string.Format("{0:00}:{1:00}", minutes, seconds);
        timeText.text = textTimer;
    }
}

