using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Codigo para el spawn de las armas, este codigo va ubicado en un GameObject vacio, se agrega un tiempo en "timeChange" que sera el tiempo que tarde en respawnear un arma y en la list se agregran las armas a spawnear.
/// </summary>
public class SpawnGun : MonoBehaviour
{

    [Tooltip("Armas que se spawnearan")]
    [SerializeField] GameObject[] arma; //Armas que se spawnearan

    [Tooltip("Tiempo que tarda en respawnear el arma despues de ser agarrada")]
    [SerializeField] float timechange;

    [Tooltip("Arma que esta activa en ese momento")]
    private GameObject currentGun;

    private Coroutine changeCorutine;

    private void Start() //Al iniciar:
    {
       
       changeActivator(); // Se activa la funcion que spawnea el arma
       changeCorutine = StartCoroutine(GunSpawner()); //Inicia la corrutina para cambiar de arma

    }

    public void changeActivator() //Funcion que spawnea el arma 
    {
       Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z); //Agarra la posicion en la que spawneara (del game object en el que esta)
       currentGun = Instantiate(arma[Random.Range(0, arma.Length)], ramdomSpawn, Quaternion.identity); //Instancia el arma en l punto indicado arriba aleatoiramente cuyo numero es la posicoin del arma en la lista

    }

    IEnumerator GunSpawner() {

        while (true)  //Corrutina activa
        {
            yield return new WaitForSeconds(timechange); //Tiempo en que respawnea

            if (currentGun == null)  //Si la arma activa "currenGun" no existe:
            {
                changeActivator(); //Activa la funcion que spawnea el arma
               
            }
        }
    }
}

