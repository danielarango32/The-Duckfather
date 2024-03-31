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

    [Space]
    [Header("Emparentamientos")]
    [SerializeField] GameObject prefabBala;
    [SerializeField] Transform canon;

    [Space]
    [Header("Bala")]
    [SerializeField] float velocidad;


    [Header("Arma Habilitada")]
    [SerializeField] public float numArma;

    [Header("Numero De Balas")]
    [SerializeField] public float numBalas;

    /*
     Lo siguiente va a esconderse
     
     */
    [Space]
    [Header("Numero De Balas Por Arma")]
    public float balPistol, balbazuca, balMetralleta, balLanza;


    [Header("WeaponcontrollerReference")]
    public GameObject WeaponController;


    private void Start()
    {
        firerateTimer = fireRate;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            numArma = 1;
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
            if (ShouldFire()) Shoot();
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
            
            WeaponController.transform.Find("ArmaBaseChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);

            Debug.Log("Armas Desactivadas");
        }
        if (numeroDeArma == 1)
        {
            WeaponController.transform.Find("ArmaBaseChild").gameObject.SetActive(true);
            //canon.transform.position = new Vector3(-0.298f, -0.011f, 1.057f);
            Debug.Log("Arma Activada Pistol");
        }
        if (numeroDeArma == 2)
        {
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(true);
            //canon.transform.position = new Vector3(-0.57f, 0.044f, 0.808f);
            Debug.Log("Arma Activada Bazuca");
        }
        if (numeroDeArma == 3)
        {
            Debug.Log("Arma Activada Metralleta");
        }
        if (numeroDeArma == 4)
        {
            Debug.Log("Arma Activada LanzaGranadas");
        }
    }



    void Shoot()
    {
        float disparo = 0;

        firerateTimer = 0;
        Debug.Log("Disparando");

        GameObject proyectil = Instantiate(prefabBala, canon.position, canon.rotation);

        Rigidbody rb = proyectil.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = transform.forward * velocidad;
        }


        disparo++;
        Ammo(disparo);
        disparo = 0;
    }

    bool Ammo(float disparo)
    {
        balPistol = balPistol - disparo;


        if (balPistol <= 0) return false;


        return false;
    }



}
