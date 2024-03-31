using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Volver : MonoBehaviourPunCallbacks

{
    [SerializeField] string scena;
    public void OnClickBack()
    {
        PhotonNetwork.ConnectUsingSettings();
        SceneManager.LoadScene(scena);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
