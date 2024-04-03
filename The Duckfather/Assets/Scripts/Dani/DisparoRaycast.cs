using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisparoRaycast : MonoBehaviour
{
    public float damage;
    public float fireRate;
    [SerializeField]Camera camera;
    [SerializeField] Transform canon;
    

    private float nextFire;

    void Update()
    {
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime; 
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && nextFire <= 0)
        {
            nextFire = 1/fireRate;


            Fire();
            Debug.Log("Disparando");
        }
    }

    void Fire()
    {

        Ray ray = new Ray(canon.transform.position, camera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);

        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            if (hit.transform.gameObject.GetComponent<LifeManager>())
            {
                //hit.transform.gameObject.GetComponent<PhotonView>().RPC("QuitarVida", RpcTarget.All, damage);
                hit.transform.gameObject.GetComponent<LifeManager>().QuitarVida(damage);
                Debug.DrawRay(hit.transform.position, hit.transform.position, Color.yellow, 2f);
            }
        }

    }
}
