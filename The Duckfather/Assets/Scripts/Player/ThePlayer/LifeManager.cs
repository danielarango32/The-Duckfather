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
    [SerializeField] Slider sliderVida;
    [SerializeField] Slider sliderEscudo;

    public bool damageRecieved;
    public float contador;

    public PlayerPhotonSoundManager playerPhotonSoundManager;

    private void Update()
    {
        //sliderVida.value = vida;

        if (damageRecieved)
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
    public void QuitarVida(float damage, PhotonMessageInfo info = default)
    {
        Debug.Log("El dano que llega es=" + damage);

        StartCoroutine(DamageRecieved());

        if (escudo>0)
        {
            escudo -= damage;

            if (escudo < 0)
            {
                escudo = 0;
            }
            
        }
        else
        {
            vida -= damage;

           // playerPhotonSoundManager.PlayHurtSFX();
        }

        if (vida <= 0 )
        {

            Destroy(this.gameObject);
        } 
    }

    [PunRPC]
    IEnumerator DamageRecieved()
    {
        damageRecieved = true;
        yield return new WaitForSeconds(2f);
        damageRecieved = false;

        yield return null;

    }  
}
