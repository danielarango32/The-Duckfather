using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creditos : MonoBehaviour
{
    [SerializeField] GameObject creditos;
    // Start is called before the first frame update
    void Start()
    {
        creditos.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // opne the popup with the credits
    
    public void OpenCredits()
    {
        creditos.SetActive(true);
    }
    
    // close the popup with the credits
    
    public void CloseCredits()
    {
        creditos.SetActive(false);
    }
}
