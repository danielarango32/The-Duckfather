using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class PlayerNameManager : MonoBehaviour
{
    
    [SerializeField] TMP_InputField PlayerNameInput;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerNameInput.text = PlayerPrefs.GetString("PlayerName");
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            PlayerNameInput.text = "Player" + Random.Range(0, 1000).ToString("0000");
            this.OnUserNameInputValueChange();
        }
    }
    public void OnUserNameInputValueChange()
    {
        
        PhotonNetwork.NickName = PlayerNameInput.text;
        PlayerPrefs.SetString("PlayerName", PlayerNameInput.text);
        
    }
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
