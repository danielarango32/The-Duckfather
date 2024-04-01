/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;

public class LoaderBoard : MonoBehaviour
{
    public GameObject pplayerHolder;
    
    [Header("Options")]
    public float refreshRate = 1f;

    [Header("UI")] public GameObject[] slots;
    
    [Space]
    public TextMeshProUGUI[] scoreText;
    public TextMeshProUGUI[] nameText;


    private void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
    }

    public void Refresh()
    {
        foreach (var slot in slots)
        {
            slot.SetActive(false);
        }

       /var sortedPlayers =
            (for player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();
        
        int i = 0;
        foreach (var player in sortedPlayers)
        {
            slots[i].SetActive(true);

            if (player.NickName == "")
            {
                player.NickName = "unmamed";
            }
            
            nameText[i].text = player.NickName;
            scoreText[i].text = player.GetScore().ToString();

            i++;
        }

    }

    private void Update()
    {
        pplayerHolder.SetActive(Input.GetKey(KeyCode.Tab));
    }
}*/
