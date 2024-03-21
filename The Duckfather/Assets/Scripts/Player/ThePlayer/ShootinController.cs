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

    [Space]
    [Header("Numero De Balas Por Arma")]
    [SerializeField] private float balPistol, balbazuca, balMetralleta, balLanza;

    private void Start()
    {
        firerateTimer = fireRate;
    }


    private void Update()
    {
        if (ShouldFire()) Shoot();
    }

    
    bool ShouldFire()
    {
        firerateTimer += Time.deltaTime;

        if(firerateTimer < fireRate) return false;

        if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;


        if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;

        return false;

    }


    void SelectorDeArma(float numeroDeArma)
    {
        if (numeroDeArma == 0)
        {
            Debug.Log("Armas Desactivadas");
        }
        if (numeroDeArma == 1)
        {
            canon.transform.position = new Vector3(-0.298f, -0.011f, 1.057f);
            Debug.Log("Arma Activada Pistol");
        }
        if (numeroDeArma == 2)
        {
            canon.transform.position = new Vector3(-0.57f, 0.044f, 0.808f);
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
        firerateTimer = 0;
        Debug.Log("Disparando");

        GameObject proyectil = Instantiate(prefabBala, canon.position,canon.rotation);

        Rigidbody rb = proyectil.GetComponent<Rigidbody>();

        if (rb !=null)
        {
            rb.velocity = transform.forward * velocidad;
        }
    }



}
