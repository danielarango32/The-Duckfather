using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vida y escudo del pato. El escudo absorbe el dano antes que la vida y ambos
/// se regeneran tras unos segundos sin recibir impactos.
///
/// Solo el dueno del pato lleva la cuenta: QuitarVida sale por RPC hacia el
/// propietario y el resto de clientes no tocan estos valores.
/// </summary>
public class LifeManager : MonoBehaviour
{
    [Header("Sistema de vida")]
    [Tooltip("Vida maxima; debe coincidir con el Max Value del slider de vida")]
    [SerializeField] private float vidaMax = 100f;

    [Tooltip("Escudo maximo; absorbe el dano antes que la vida")]
    [SerializeField] private float escudoMax = 100f;

    [Tooltip("Segundos sin recibir dano antes de que arranque la regeneracion")]
    [SerializeField] private float tiempoParaRegen = 5f;

    [Tooltip("Puntos regenerados por segundo: primero escudo, luego vida")]
    [SerializeField] private float cantidadDeRegeneracion = 20f;

    [Header("UI de vida")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private Slider sliderEscudo;

    [Tooltip("Barra rellenable de UI/Barra/backgrounVida/vida, superpuesta al slider")]
    [SerializeField] private Image healthBarImage;

    [SerializeField] private GameObject ui;

    [Header("Red")]
    public PhotonView PV;

    // Lo usara el efecto de dano; se deja cableado desde el prefab.
    public PlayerPhotonSoundManager playerPhotonSoundManager;

    private PlayerManager playerManager;

    private float vida;
    private float escudo;

    // Segundos desde el ultimo impacto. Sustituye a la pareja
    // danorecibido/contador con corrutina de 2 s: al encadenar dos golpes, la
    // corrutina del primero bajaba la bandera y la regeneracion arrancaba antes
    // de tiempo.
    private float tiempoSinDano;

    public float Vida => vida;
    public float Escudo => escudo;

    private void Awake()
    {
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        // Va en Awake y no en Start porque PlayerSetUp desactiva este
        // componente para los patos remotos: si su Start ganaba la carrera, el
        // Start de aqui no llegaba a correr y la UI ajena se quedaba en pantalla.
        if (!PV.IsMine)
        {
            Destroy(ui);
        }
    }

    private void Start()
    {
        vida = vidaMax;
        escudo = escudoMax;
        tiempoSinDano = tiempoParaRegen;

        ConfigurarSlider(sliderVida, vidaMax);
        ConfigurarSlider(sliderEscudo, escudoMax);
        RefrescarUI();
    }

    private static void ConfigurarSlider(Slider slider, float maximo)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = maximo;
    }

    private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }

        tiempoSinDano += Time.deltaTime;

        if (tiempoSinDano >= tiempoParaRegen)
        {
            Regenerar(cantidadDeRegeneracion * Time.deltaTime);
        }
    }

    /// <summary>
    /// Punto de entrada del dano: enruta el golpe al dueno del pato, que es
    /// quien lleva la cuenta de vida y escudo.
    /// </summary>
    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(QuitarVida), PV.Owner, damage);
    }

    [PunRPC]
    public void QuitarVida(float Dano, PhotonMessageInfo info = default)
    {
        if (!PV.IsMine)
        {
            return;
        }

        tiempoSinDano = 0f;

        // El escudo absorbe primero y solo el sobrante llega a la vida.
        float restante = Dano;

        if (escudo > 0f)
        {
            float absorbido = Mathf.Min(escudo, restante);
            escudo -= absorbido;
            restante -= absorbido;
        }

        // La barra se refrescaba ANTES de restar el dano, asi que siempre iba
        // un golpe por detras.
        vida = Mathf.Max(0f, vida - restante);
        RefrescarUI();

        if (vida <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Reparte la regeneracion: primero rellena el escudo y lo que sobra va a
    /// la vida.
    /// </summary>
    private void Regenerar(float cantidad)
    {
        if (cantidad <= 0f || (escudo >= escudoMax && vida >= vidaMax))
        {
            return;
        }

        if (escudo < escudoMax)
        {
            float aplicado = Mathf.Min(escudoMax - escudo, cantidad);
            escudo += aplicado;
            cantidad -= aplicado;
        }

        if (cantidad > 0f && vida < vidaMax)
        {
            vida = Mathf.Min(vidaMax, vida + cantidad);
        }

        RefrescarUI();
    }

    private void RefrescarUI()
    {
        if (sliderVida != null)
        {
            sliderVida.value = vida;
        }

        if (sliderEscudo != null)
        {
            sliderEscudo.value = escudo;
        }

        // La barra rellenable solo se tocaba al recibir dano, asi que al
        // regenerar se quedaba congelada y contradecia al slider.
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = vidaMax > 0f ? vida / vidaMax : 0f;
        }
    }

    private void Die()
    {
        playerManager.Die();
    }
}
