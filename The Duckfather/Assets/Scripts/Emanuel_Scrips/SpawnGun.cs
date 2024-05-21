using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGun : MonoBehaviour
{
    public Transform spawn;

    [SerializeField] GameObject[] arma;
    [SerializeField] float timechange;
   

    public GameObject currentGun;

    private Coroutine changeCorutine;

    private void Start()
    {
       /* Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GameObject gameObject = Instantiate(arma[Random.Range(0, arma.Length)], ramdomSpawn, Quaternion.identity);
        WeaponOn = true;
       changeCorutine = StartCoroutine(GunSpawner());*/
       changeActivator();
        //changeCorutine = StartCoroutine(GunSpawner());

    }

    public void Update()
    {
       

        
        if(currentGun != null)
        {
            StopCoroutine(changeCorutine);
        }
        else
        {
            changeCorutine = StartCoroutine(GunSpawner());
            
            // StartCoroutine(GunSpawner());
        }
        
    }



    public void changeActivator()
    {
        Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        currentGun = Instantiate(arma[Random.Range(0, arma.Length)], ramdomSpawn, Quaternion.identity);
        // StartCoroutine(GunSpawner());

        /* do
         {
             Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z);
             currentGun = Instantiate(arma[Random.Range(0, arma.Length)], ramdomSpawn, Quaternion.identity);

         }
         while (currentGun != null);*/







    }

   /* public void HandleWeaponPickedUp()
    {
        WeaponCollision weaponCollision = currentGun.GetComponent<WeaponCollision>();
        if (weaponCollision != null)
        {
           StartCoroutine(GunSpawner());
        }
    }
   */
   

    IEnumerator GunSpawner() { 


                yield return new WaitForSeconds(timechange);
           
              
                changeActivator();
        StopCoroutine(changeCorutine);


        // Destroy(gameObject);
        //  WeaponOn = false;



    }
}

