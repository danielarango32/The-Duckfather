using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoger_Arma : MonoBehaviour
{
    GameObject Arma;
    private void Start()
    {
        Arma = GameObject.Find("Arma");

    }
    private void Update()
    {
        Arma.transform.position = transform.position;
        Arma.transform.rotation = transform.rotation;
    }
}
