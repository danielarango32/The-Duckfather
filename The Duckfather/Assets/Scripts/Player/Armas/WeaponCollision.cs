using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [Header("Numero de Arma")]
    [SerializeField] float numArma;


    public SpawnGun spawnChanger;

    public event Action OnWeaponPickedUp;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<ShootinController>().SelectorDeArma(numArma);
           
            Debug.Log("jugador reconocido");
            
            

            this.gameObject.SetActive(false);
            NetworkCleanup.Remove(this.gameObject);


           // spawnChanger.changeActivator();
        }
        
    }
}
