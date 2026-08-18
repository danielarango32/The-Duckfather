using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    [Header("Sistema de vida")]
    //[SerializeField] float vida = 100;
    [SerializeField] float escudo = 100;
    [SerializeField] float tiempoParaRegen;
    [SerializeField] float cantidadDeRegeneracion;
    
    public bool isLocalPlayer;

    [SerializeField] Image healthBarImage;
    
    public RectTransform healthBar;
    public RectTransform shieldBar;
    
    private float originalHealthBarSize;
    private float originalShieldBarSize;


    [Header("UI de vida")]
    [SerializeField] Slider sliderVida;
    [SerializeField] Slider sliderEscudo;

    public bool danorecibido;
    public float contador;
    
    [SerializeField] GameObject ui;
    
    public PhotonView PV;
    
    PlayerManager playerManager;

    public PlayerPhotonSoundManager playerPhotonSoundManager;

    const float maxHealth = 100;
    float vida = maxHealth;

    private void Awake()
    {
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        /*originalHealthBarSize = healthBar.sizeDelta.x;
        originalHealthBarSize = sliderVida.GetComponent<RectTransform>().sizeDelta.x;
        
        
        originalShieldBarSize = shieldBar.sizeDelta.x;
        originalShieldBarSize = sliderEscudo.GetComponent<RectTransform>().sizeDelta.x;*/
        
        if (!PV.IsMine)
        {
            Destroy(ui);
        }
        
        
    }
    private void Update()
    {
        
        
        sliderVida.value = vida;
        sliderEscudo.value = escudo; 
        

        if (danorecibido)
        {
            contador = 0;
        }
        else
        {
            contador += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (contador >= tiempoParaRegen)
        {
            
            ObtenerVida();
        }
    }


    [PunRPC]
    void ObtenerVida()
    {
        if (vida < 100)
        {
            vida = vida + cantidadDeRegeneracion / 100;
        }
        else
        {
            vida = 100;
        }
       
    }
    [PunRPC]
    void ObtenerEscudo()
    {

    }
    
    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(QuitarVida), PV.Owner, damage);
    }
    
    [PunRPC]
    public void QuitarVida(float Dano, PhotonMessageInfo info = default)
    {
        if(!PV.IsMine) return;
        
        Debug.Log("El dano que llega es=" + Dano);

        StartCoroutine(Danorecibido());

          
           /* if (escudo > 0)
            {
                escudo -= Dano;

                shieldBar.sizeDelta = new Vector2(originalShieldBarSize * escudo / 100, shieldBar.sizeDelta.y);

                if (escudo < 0)
                {

                    escudo = 0;
                }

            }
            else*/
            //{
                //healthBar.sizeDelta = new Vector2(originalHealthBarSize * vida / 100, healthBar.sizeDelta.y);

                healthBarImage.fillAmount = vida / maxHealth;
                vida -= Dano;
                //playerPhotonSoundManager.PlayHurtSFX();
            //}
            if (vida <= 0)
            {
                Die();
            }

    }

    [PunRPC]
    IEnumerator Danorecibido()
    {
        danorecibido = true;
        yield return new WaitForSeconds(2f);
        danorecibido = false;

        yield return null;

    }  
    
    void Die()
    {
        playerManager.Die();
    }
    
}
