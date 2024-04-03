using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Photon.Pun;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    [Header("Sistema de vida")]
    [SerializeField] float vida = 100;
    [SerializeField] float escudo = 0;
    [SerializeField] float tiempoParaRegen;
    [SerializeField] float cantidadDeRegeneracion;

    public bool dañorecibido;
    public float contador;
    private void Update()
    {
        

        if (dañorecibido)
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
    public void QuitarVida(float Daño, PhotonMessageInfo info = default)
    {
        Debug.Log("El daño que llega es=" + Daño);

        StartCoroutine(Dañorecibido());

        if (escudo>0)
        {
            escudo -= Daño;

            if (escudo < 0)
            {
                escudo = 0;
            }
            
        }
        else
        {
            vida -= Daño;
        }

        if (vida <= 0 )
        {
            Destroy(this.gameObject);
        } 
    }

    [PunRPC]
    IEnumerator Dañorecibido()
    {
        dañorecibido = true;
        yield return new WaitForSeconds(2f);
        dañorecibido = false;

        yield return null;

    }
}
