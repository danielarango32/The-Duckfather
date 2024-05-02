using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    
     public Transform[] spawnPoints;
    
    void Awake()
    {
        instance = this;
        spawnPoints = GameObject.Find("Spawn").GetComponentsInChildren<Transform>();
    }
    
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
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

