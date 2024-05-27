using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
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
        // Esperar a que todos los jugadores se unan antes de iniciar el temporizador
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Update()
    {
        if (timerStarted)
        {
            timer -= Time.deltaTime;
            UpdateTimerText(timer);

            if (timer <= 0f && PhotonNetwork.IsMasterClient)
            {
                // Finalizar la partida y la sesión
                EndGame();
            }
        }
        Debug.Log(timer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Cuando un nuevo jugador se une, verificar si todos los jugadores han entrado
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            // Iniciar el temporizador
            timer = timerDuration;
            timerStarted = true;

            // Notificar a todos los clientes que el temporizador ha comenzado
            photonView.RPC("StartTimer", RpcTarget.All, timer);
        }
    }

    [PunRPC]
    private void StartTimer(float duration)
    {
        timer = duration;
        timerStarted = true;
    }

    private void EndGame()
    {
        // Finalizar la partida y la sesión
        PhotonNetwork.DestroyAll();
        PhotonNetwork.LeaveRoom();
    }

    private void UpdateTimerText(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer - minutes * 60f);
        string textTimer = string.Format("{0:00}:{1:00}", minutes, seconds);
        timeText.text = textTimer;
    }
}