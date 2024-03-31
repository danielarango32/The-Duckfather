using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Logo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartChangeScene();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // change scene to main menu after 5 seconds
    public void StartChangeScene()
    {
        StartCoroutine(ChangeScene());
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Menu");
    }
    
    
}
