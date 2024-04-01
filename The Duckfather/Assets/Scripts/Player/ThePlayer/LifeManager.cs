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
    
    [Header("UI de vida")]
    public Slider sliderVida;
    public Slider sliderEscudo;

    public bool danorecibido;
    public float contador;
    
    public int health;
    private void Update()
    {
        

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
    
    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;
        
        sliderVida.value = health;
        
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
