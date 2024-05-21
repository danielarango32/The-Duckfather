using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UserNameadisplay : MonoBehaviour
{
    [SerializeField] PhotonView PlayerPV;
    [SerializeField] TMP_Text UserNameText;

    private void Start()
    {
        if (PlayerPV.IsMine)
        {
            gameObject.SetActive(false);
        }
        UserNameText.text = PlayerPV.Owner.NickName;
    }

}
