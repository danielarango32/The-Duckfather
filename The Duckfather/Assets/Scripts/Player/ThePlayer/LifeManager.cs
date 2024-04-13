using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    [Header("Sistema de vida")]
    [SerializeField] float vida = 100;
    [SerializeField] float escudo = 0;
    [SerializeField] float tiempoParaRegen;
    [SerializeField] float cantidadDeRegeneracion;
    
    public bool isLocalPlayer;
    
    private float originalHealthBarSize;
    private float originalShieldBarSize;


    [Header("UI de vida")]
    [SerializeField] Slider sliderVida;
    [SerializeField] Slider sliderEscudo;

    public bool danorecibido;
    public float contador;

    private void Start()
    {
        originalHealthBarSize = sliderVida.GetComponent<RectTransform>().sizeDelta.x;
        sliderVida.maxValue = vida;
        
        originalShieldBarSize = sliderEscudo.GetComponent<RectTransform>().sizeDelta.x;
        sliderEscudo.maxValue = escudo;
        
    }
    private void Update()
    {
        //sliderVida.GetComponent<RectTransform>().sizeDelta = new Vector2(originalHealthBarSize * vida / 100, sliderVida.GetComponent<RectTransform>().sizeDelta.y);
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
    [PunRPC]
    public void QuitarVida(float Dano, PhotonMessageInfo info = default)
    {
        Debug.Log("El dano que llega es=" + Dano);

        StartCoroutine(Danorecibido());
        
        sliderVida.GetComponent<RectTransform>().sizeDelta = new Vector2(originalHealthBarSize * vida / 100, sliderVida.GetComponent<RectTransform>().sizeDelta.y);

        if (escudo>0)
        {
            escudo -= Dano;

            if (escudo < 0)
            {
                escudo = 0;
            }
            
        }
        else
        {
            vida -= Dano;
        }

        if (vida <= 0 )
        {
            if (isLocalPlayer)
            {
                Roomandlobbymanager.Instance.SpawnPlayer();
            }
            
            
            //Roomandlobbymanager.Instance.
            //Roomandlobbymanager.Instance

            Destroy(this.gameObject);
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
}
