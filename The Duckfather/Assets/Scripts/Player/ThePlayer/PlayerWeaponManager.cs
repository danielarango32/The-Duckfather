using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    /*
     Lo que quiero es que el weapon manager me ayude a colocar el arma donde debe estar en el jugador
     Quiero que me ayude a recoger el arma, pero solo si estoy en el rango del arma y hundo la letra "E"
     Ademas quiero poder tener 2 armas al mismo tiempo y cambiar entre ellas, o sea recojo la pistola base, 
     si recojo un powerUp, poder cambiar entre ambas armas

     Quiero que mi limite de armas cargadas sean 2, o sea que si me paro sobre otra arma no la pueda recoger, hasta haber gastado
     el powerUp
     
     EXISTENS 4 ARMAS 
     Pistola - Bazuca - Metralleta - LanzaGranadas
     
     */

    [Header("RayCast")]
    [Range(2,3)]
    public float rayRadio;
    public bool armadetected;
    //public Transform checker;

    [Space]
    [Header("Parent Arma")]
    [SerializeField] private Transform parent;

    [Header("Parent Power Up")]
    [SerializeField] private Transform PUparent;



    /*[Space]
    [Header("Lista de Armas")]
    public List<Armas> armas = new List<Armas> ();
    */


    private GameObject detectedGO;

    private void Update()
    {
        RecogerArmaPlusRayCast(detectedGO);
        GetComponent<SphereCollider>().radius = rayRadio;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Arma") || other.gameObject.CompareTag("PowerUp"))
        {
            Debug.Log("ArmaDetectd");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Arma") || other.gameObject.CompareTag("PowerUp"))
        {
            Debug.Log("Fuera del arma");
            detectedGO = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Arma") || other.gameObject.CompareTag("PowerUp"))
        {
            Debug.Log("Parado en el arma");
            detectedGO = other.gameObject;
        }
    }



    private void RecogerArmaPlusRayCast(GameObject detectedWeapon)
    {
        armadetected = Physics.CheckSphere(parent.position, rayRadio);

        if (armadetected && Input.GetKeyDown(KeyCode.E) && detectedGO != null) 
        {
            if (detectedGO.CompareTag("Arma"))
            {
                detectedGO.transform.SetParent(parent.transform, false);
                detectedGO.transform.localPosition = Vector3.zero;
                detectedGO.transform.localRotation = Quaternion.identity;
                detectedGO.GetComponent<Collider>().enabled = false;
            }
            if (detectedGO.CompareTag("PowerUp"))
            {
                detectedGO.transform.SetParent(PUparent.transform, false);
                detectedGO.transform.localPosition = Vector3.zero;
                detectedGO.transform.localRotation = Quaternion.identity;
                detectedGO.GetComponent<Collider>().enabled = false;
            }
            

        }
        
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;

        Gizmos.DrawWireSphere(parent.position, rayRadio);

    }
    
}
