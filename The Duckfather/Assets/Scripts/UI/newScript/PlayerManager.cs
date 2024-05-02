using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
public class PlayerManager : MonoBehaviour
{
    
    PhotonView PV;
    
    GameObject controller;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
        }
        
    }
    
    void CreateController()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate("Pato rolo", spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
