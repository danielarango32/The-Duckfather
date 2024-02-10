using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoger_Arma : MonoBehaviour
{
    //VISUALIZACION CHECKER
    public float radio;

    //CHECKER POSITION
    private Transform checkerpos;

    //DATOS
    public LayerMask Armas;
    public bool armaDetected;

    //POSICIONPARENTARMA
    [SerializeField] private Transform parentArma;

    private void Update()
    {
        RecogerArmas();
    }

    void RecogerArmas()
    {
        armaDetected = Physics.CheckSphere(checkerpos.position, radio, Armas);
        
        if (armaDetected && Input.GetKeyDown(KeyCode.E))
        {
            
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, radio);
    }

}
