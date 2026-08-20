using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
    [SerializeField] Transform multiAimConstraint;

    [Space]
    [Header("UI")]
    [SerializeField] private GameObject balasUI;
    [SerializeField] private TMP_Text numBalasUI;
    [SerializeField] private GameObject bazucaUI;
    [SerializeField] private GameObject revolverUI;
    [SerializeField] private GameObject thomsonUI;
    [SerializeField] private GameObject pistolaUI;

    [Header("PhotonView")]
    public PhotonView photonView;


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


    [Header("SFX")]
    
    public int shootSFXIndex = 0;
   private int grabSFXIndex = 0;
    private PlayerPhotonSoundManager playerPhotonSoundManager;

    private void Awake()
    {
        // Se resolvian en Start(), pero SelectorDeArma() puede llegar desde
        // WeaponCollision.OnTriggerEnter antes de que Start() haya corrido.
        photonView = GetComponent<PhotonView>();
        playerPhotonSoundManager = GetComponent<PlayerPhotonSoundManager>();
    }

    private void Start()
    {
        firerateTimer = fireRate;
    }

    /*
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
    */


    private void FixedUpdate()
    {
        if (!photonView.IsMine) return; // Si no es el jugador local, no hacer nada



        //posición del VFX del disparo
        if (vfxDisparo.transform.position != canon.position)
        {
            vfxDisparo.transform.position = canon.position;
        }
        
        //posición del VFX del disparo de la bazuca
        if (vfxDisparoBazuca.transform.position != canon.position)
        {
            vfxDisparoBazuca.transform.position = canon.position;
        }
        


        if (numArma != 0)
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
        if (numBalas == 0)
        {
            numBalasUI.text = "";
        }
        else
        {
            numBalasUI.text = numBalas.ToString();
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


    /// <summary>
    /// Punto de entrada local: aplica el arma, suena el cambio y lo propaga.
    /// Solo actua sobre el jugador propio; el trigger de recogida salta en
    /// todos los clientes, pero al resto el arma le llega por SyncWeaponChange.
    /// </summary>
    public void SelectorDeArma(float numeroDeArma)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        ApplyWeapon(numeroDeArma);
        playerPhotonSoundManager.PlayGrabSFX(grabSFXIndex);
        photonView.RPC(nameof(SyncWeaponChange), RpcTarget.Others, numeroDeArma);
    }

    /// <summary>
    /// Aplica el arma sin tocar la red. La usan tanto el jugador propio como el
    /// RPC de sincronizacion, y por eso aqui no se emite ningun RPC.
    /// </summary>
    private void ApplyWeapon(float numeroDeArma)
    {
        shootSFXIndex = (int)numeroDeArma;
        grabSFXIndex = (int)numeroDeArma;

        numArma = numeroDeArma;
        if (numeroDeArma == 0)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            
            bazucaUI.gameObject.SetActive(false);
            revolverUI.gameObject.SetActive(false);
            thomsonUI.gameObject.SetActive(false);
            pistolaUI.gameObject.SetActive(false);
            

            //Debug.Log("Armas Desactivadas");
        }
        if (numeroDeArma == 1)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(true);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);

            balasUI.gameObject.SetActive(true);
            pistolaUI.gameObject.SetActive(true);
            bazucaUI.gameObject.SetActive(false);
            revolverUI.gameObject.SetActive(false);
            thomsonUI.gameObject.SetActive(false);
            
            canon = WeaponController.transform.Find("PistolaChild").gameObject.transform;
            fireRate = 0.5f;
            velocidad = 50;
            damage = 10;
            numBalas = 10000;

            //Debug.Log("Arma Activada Pistol");
        }
        if (numeroDeArma == 2)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(true);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            balasUI.gameObject.SetActive(true);
            pistolaUI.gameObject.SetActive(false);
            bazucaUI.gameObject.SetActive(true);
            revolverUI.gameObject.SetActive(false);
            thomsonUI.gameObject.SetActive(false);
            
            canon = WeaponController.transform.Find("BazucaChild").gameObject.transform;
            fireRate = 1.5f;
            velocidad = 90;
            numBalas = 10;  
            //Debug.Log("Arma Activada Bazuca");
        }
        if (numeroDeArma == 3)
        {
            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(true);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(false);
            balasUI.gameObject.SetActive(true);
            pistolaUI.gameObject.SetActive(false);
            bazucaUI.gameObject.SetActive(false);
            revolverUI.gameObject.SetActive(true);
            thomsonUI.gameObject.SetActive(false);
            
            canon = WeaponController.transform.Find("RevolverChild").gameObject.transform;
            fireRate = 0.9f;
            velocidad = 70;
            damage = 25;
            numBalas = 16;
            //Debug.Log("Arma Activada Revolver");
        }
        if (numeroDeArma == 4)
        {

            WeaponController.transform.Find("RevolverChild").gameObject.SetActive(false);
            WeaponController.transform.Find("PistolaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("BazucaChild").gameObject.SetActive(false);
            WeaponController.transform.Find("ThomsonChild").gameObject.SetActive(true);
            balasUI.gameObject.SetActive(true);
            pistolaUI.gameObject.SetActive(false);
            bazucaUI.gameObject.SetActive(false);
            revolverUI.gameObject.SetActive(false);
            thomsonUI.gameObject.SetActive(true);
            
            canon = WeaponController.transform.Find("ThomsonChild").gameObject.transform;
            fireRate = 0.2f;
            velocidad = 70;
            damage = 5;
            numBalas = 80;
            //Debug.Log("Arma Activada Thomson");
        }
        if (!photonView.IsMine) // Verifica si es el jugador local
        {
            bazucaUI.gameObject.SetActive(false);
            revolverUI.gameObject.SetActive(false);
            thomsonUI.gameObject.SetActive(false);
            pistolaUI.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void SyncWeaponChange(float weaponNumber)
    {
        // Aplica sin reenviar: aqui se cerraba el bucle A -> B -> A -> B...
        ApplyWeapon(weaponNumber);
    }

    void Shoot()
    {

        playerPhotonSoundManager.PlayShootSFX(shootSFXIndex);

        float disparo = 0;

        firerateTimer = 0;
        Debug.Log("Disparando");

        GameObject proyectil;

        proyectil = PhotonNetwork.Instantiate(prefabBala.name, canon.position, target.rotation);
        PhotonNetwork.Instantiate(vfxDisparoBazuca.name, canon.position, Quaternion.identity);
        
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
        playerPhotonSoundManager.PlayShootSFX(shootSFXIndex);
        float disparo = 0;
        firerateTimer = 0;

        //Ray ray = new Ray(canon.transform.position, target.transform.forward);
        Ray ray = new Ray(canon.transform.position, multiAimConstraint.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);

        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitDisparo.name, hit.point, Quaternion.identity);
            PhotonNetwork.Instantiate(vfxDisparo.name, canon.position, Quaternion.identity);


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


        if (numBalas <= 0)
        {
            SelectorDeArma(0);
        }


        return false;
    }



}
