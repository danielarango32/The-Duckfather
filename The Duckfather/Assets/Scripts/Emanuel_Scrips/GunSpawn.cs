using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scrip para el spawn aleatorio de las armas
/// </summary>
public class GunSpawn : MonoBehaviour
{
    public Transform spawn;

    public GameObject[] arma;
    public GameObject arma2;
  

    private void Start()
    {


        GunChanger();


    }

    
    public void GunChanger()
    {
        Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z);


        GameObject arma2 = Instantiate(arma[Random.Range(0, 3)], ramdomSpawn, Quaternion.identity);

        arma2.AddComponent<NewGun>();
       
    }


   /* IEnumerator NewGun()
    {
        yield return new WaitForSeconds(2f);
        Destroy(arma[]);
    }*/

}
public class NewGun : GunSpawn
{
    public NewGun neww;
     private void Start()
    {
        StartCoroutine(NewGunP());
    }

    
    IEnumerator NewGunP()
    {
        

        yield return new WaitForSeconds(5f);
        //Vector3 ramdomSpawn = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //Instantiate(arma[Random.Range(0, 3)], neww.arma2.ramdomSpawn, Quaternion.identity);
        // GunChanger();
        GunChanger();
        Object.Destroy(this.gameObject);
       

    }
}
