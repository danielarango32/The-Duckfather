using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootinController : MonoBehaviour
{
    [Header("Shooting Attributes")]
    [Space]

    [Header("FireRate")]
    public float fireRate = 0.3f;
    public float firerateTimer;
    [SerializeField] bool semiAuto;

    [Header("Bala")]
    [SerializeField] float velocidad;
    [SerializeField] float damage;


    [Header("Arma Habilitada")]
    [SerializeField] public float numArma;

    [Header("Numero De Balas")]
    [SerializeField] public float numBalas;

    [Space]
    [Header("Emparentamientos")]
    [SerializeField] GameObject prefabBala;
    [SerializeField] Transform canon;
    [SerializeField] Transform target;

    [Space]
    

    /*
     Lo siguiente va a esconderse
     
     
    [Space]
    [Header("Numero De Balas Por Arma")]
    public float balPistol, balbazuca, balMetralleta, balLanza;
    */

    [Header("WeaponcontrollerReference")]
    public GameObject WeaponController;

    [Space]
    [Header("VFX")]
    [SerializeField] GameObject vfxDisparo;
    [SerializeField] GameObject hitDisparo;
    [SerializeField] GameObject vfxDisparoBazuca;
    [SerializeField] GameObject hitDisparoBazuca;
    private void Start()
    {
        firerateTimer = fireRate;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            numArma = 0;
            SelectorDeArma(numArma);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            numArma = 2;
            SelectorDeArma(numArma);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            numArma = 3;
            SelectorDeArma(numArma);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            numArma = 4;
            SelectorDeArma(numArma);
        }

    }
    private void FixedUpdate()
    {
        if(numArma != 0)
        {
            //Debug.Log("Entra al disparo");
            if (numArma == 2)
            {
                //Debug.Log("Puede disparar la bazuca");
                if (ShouldFire()) Shoot();
            }
            else
            {
                if (ShouldFire()) FireRaycast();
                //Debug.Log("Puede disparar cualquier otra arma");
            }
        }
        
    }


    bool ShouldFire()
    {
        firerateTimer += Time.deltaTime;

        if (firerateTimer < fireRate) return false;

        if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;


        if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;

        return false;

    }


    public void SelectorDeArma(float numeroDeArma)
    {
        numArma = numeroDeArma;
        if (numeroDeArma == 0)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            
            //Debug.Log("Armas Desactivadas");
        }
        if (numeroDeArma == 1)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(true);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            canon = WeaponController.transform.Find("PistolaChild").gameObject.transform;
            fireRate = 0.3f;
            velocidad = 50;
            damage = 25;

            //Debug.Log("Arma Activada Pistol");
        }
        if (numeroDeArma == 2)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(true);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            canon = WeaponController.transform.Find("BazucaChild").gameObject.transform;
            fireRate = 1.5f;
            velocidad = 90;

            //Debug.Log("Arma Activada Bazuca");
        }
        if (numeroDeArma == 3)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(true);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            canon = WeaponController.transform.Find("RevolverChild").gameObject.transform;
            fireRate = 0.85f;
            velocidad = 70;
            damage = 20;

            //Debug.Log("Arma Activada Metralleta");
        }
        if (numeroDeArma == 4)
        {

            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(true);
            canon = WeaponController.transform.Find("ThomsonChild").gameObject.transform;
            fireRate = 0.2f;
            velocidad = 70;
            damage = 10;
            //Debug.Log("Arma Activada LanzaGranadas");
        }
    }



    void Shoot()
    {
        float disparo = 0;

        firerateTimer = 0;
        Debug.Log("Disparando");

        GameObject proyectil;

        proyectil = PhotonNetwork.Instantiate(prefabBala.name, canon.position, target.rotation);
        //Offline
        //proyectil = Instantiate(prefabBala, canon.position, target.rotation);

        Rigidbody rb = proyectil.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = transform.forward * velocidad;
        }


        disparo++;
        Ammo(disparo);
        disparo = 0;
    }

    void FireRaycast()
    {

        float disparo = 0;
        firerateTimer = 0;

        Ray ray = new Ray(canon.transform.position, target.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);

        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitDisparo.name, hit.point, Quaternion.identity);

            if (hit.transform.gameObject.GetComponent<LifeManager>())
            {
                //online
                hit.transform.gameObject.GetComponent<PhotonView>().RPC("QuitarVida", RpcTarget.All, damage);

                //offline
                //hit.transform.gameObject.GetComponent<LifeManager>().QuitarVida(damage);
                Debug.DrawRay(hit.transform.position, hit.transform.position, Color.yellow, 2f);
            }
        }

        disparo++;
        Ammo(disparo);
        disparo = 0;

    }

    bool Ammo(float disparo)
    {
        numBalas = numBalas - disparo;


        if (numBalas <= 0) return false;


        return false;
    }



}
