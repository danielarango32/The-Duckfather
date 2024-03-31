using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Carga : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private Slider slider;
    
    public void LoadLevelBtn(string LeveltoLoad)
    {
        MainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        
        StartCoroutine(LoadLevel(LeveltoLoad));
    }
    
    IEnumerator LoadLevel(string LeveltoLoad)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(LeveltoLoad);
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            yield return null;
        }
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
