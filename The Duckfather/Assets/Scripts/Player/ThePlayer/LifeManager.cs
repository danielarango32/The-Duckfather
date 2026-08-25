using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vida, escudo, flash de golpe y muerte del pato. El escudo absorbe el dano
/// antes que la vida y ambos se regeneran tras unos segundos sin recibir
/// impactos.
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

    [Header("Flash de golpe")]
    [Tooltip("Tinte cuando el golpe llega a la vida")]
    [SerializeField] private Color colorFlashVida = Color.red;

    [Tooltip("Tinte cuando el golpe lo absorbe por completo el escudo")]
    [SerializeField] private Color colorFlashEscudo = Color.cyan;

    [Tooltip("Duracion del tinte en segundos")]
    [SerializeField] private float duracionFlash = 0.15f;

    [Tooltip("Multiplicador HDR del brillo emisivo durante el flash; a mas alto, mas notorio")]
    [SerializeField] private float intensidadFlash = 6f;

    [Header("Muerte")]
    [Tooltip("Prefab de red instanciado en el punto de muerte (particulas de plumas)")]
    [SerializeField] private GameObject deathVfx;

    [Tooltip("Segundos que el cuerpo permanece oculto antes de reaparecer")]
    [SerializeField] private float respawnDelay = 2f;

    [Header("Red")]
    public PhotonView PV;

    public PlayerPhotonSoundManager playerPhotonSoundManager;

    private PlayerManager playerManager;
    private PlayerMovement playerMovement;
    private ShootinController shootinController;

    // Solo el SkinnedMeshRenderer del cuerpo: se tinta con el flash de golpe y
    // se apaga en la muerte. Las armas se guardan aparte porque no comparten
    // shader ni la propiedad _BaseColor.
    private Renderer[] renderersDelCuerpo;
    private Color[] coloresOriginalesDelCuerpo;
    private Color[] emisionesOriginalesDelCuerpo;
    private Renderer[] renderersDelPato;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock flashBlock;
    private Coroutine flashEnCurso;

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
        playerMovement = GetComponent<PlayerMovement>();
        shootinController = GetComponent<ShootinController>();

        // El campo del inspector viene sin asignar en Pato 2 y Pato 3: se
        // resuelve aqui para que el SFX de golpe/muerte funcione en los tres
        // skins en vez de solo en el primero.
        if (playerPhotonSoundManager == null)
        {
            playerPhotonSoundManager = GetComponent<PlayerPhotonSoundManager>();
        }

        CachearRenderers();

        // Va en Awake y no en Start porque PlayerSetUp desactiva este
        // componente para los patos remotos: si su Start ganaba la carrera, el
        // Start de aqui no llegaba a correr y la UI ajena se quedaba en pantalla.
        if (!PV.IsMine)
        {
            Destroy(ui);
        }
    }

    /// <summary>
    /// Pato 1 trae la malla inline en el prefab; Pato 2 y 3 la sacan de un FBX
    /// anidado (Rig_Duck_2). GetComponentsInChildren cubre los tres casos igual.
    /// </summary>
    private void CachearRenderers()
    {
        renderersDelPato = GetComponentsInChildren<Renderer>(true);

        List<Renderer> cuerpo = new List<Renderer>();
        foreach (Renderer r in renderersDelPato)
        {
            if (r is SkinnedMeshRenderer)
            {
                cuerpo.Add(r);
            }
        }
        renderersDelCuerpo = cuerpo.ToArray();

        coloresOriginalesDelCuerpo = new Color[renderersDelCuerpo.Length];
        emisionesOriginalesDelCuerpo = new Color[renderersDelCuerpo.Length];
        for (int i = 0; i < renderersDelCuerpo.Length; i++)
        {
            coloresOriginalesDelCuerpo[i] = renderersDelCuerpo[i].sharedMaterial.GetColor(BaseColorId);
            emisionesOriginalesDelCuerpo[i] = renderersDelCuerpo[i].sharedMaterial.GetColor(EmissionColorId);
        }

        flashBlock = new MaterialPropertyBlock();
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

        if (playerPhotonSoundManager != null)
        {
            playerPhotonSoundManager.PlayHurtSFX();
        }

        // Se manda a todos (no solo al dueno) para que el disparador tambien
        // vea el impacto en el pato al que le disparo.
        bool tomoVida = restante > 0f;
        PV.RPC(nameof(FlashDeGolpe), RpcTarget.All, tomoVida);

        if (vida <= 0f)
        {
            Die();
        }
    }

    [PunRPC]
    private void FlashDeGolpe(bool tomoVida)
    {
        if (flashEnCurso != null)
        {
            StopCoroutine(flashEnCurso);
        }

        flashEnCurso = StartCoroutine(Flash(tomoVida ? colorFlashVida : colorFlashEscudo));
    }

    private IEnumerator Flash(Color color)
    {
        AplicarColorCuerpo(color);
        yield return new WaitForSeconds(duracionFlash);
        AplicarColorCuerpo(null);
        flashEnCurso = null;
    }

    /// <summary>
    /// MaterialPropertyBlock en vez de renderer.material: evita instanciar una
    /// copia del material del pato en cada impacto (las dos mallas de Pato 1
    /// comparten el mismo material).
    ///
    /// Tintar solo _BaseColor se notaba poco: es el albedo, asi que la luz de
    /// la escena y las sombras lo atenuan segun el angulo de camara. Se suma
    /// _EmissionColor a intensidadFlash (HDR, por encima de 1) porque la
    /// emision se agrega al resultado ya iluminado y no depende de la luz
    /// ambiente ni del angulo desde el que se mire.
    /// </summary>
    private void AplicarColorCuerpo(Color? colorOverride)
    {
        for (int i = 0; i < renderersDelCuerpo.Length; i++)
        {
            Color baseColor = colorOverride ?? coloresOriginalesDelCuerpo[i];
            Color emision = colorOverride.HasValue
                ? colorOverride.Value * intensidadFlash
                : emisionesOriginalesDelCuerpo[i];

            flashBlock.SetColor(BaseColorId, baseColor);
            flashBlock.SetColor(EmissionColorId, emision);
            renderersDelCuerpo[i].SetPropertyBlock(flashBlock);
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
        StartCoroutine(SecuenciaDeMuerte());
    }

    /// <summary>
    /// Cuerpo oculto en todos los clientes, VFX de plumas en el punto de
    /// muerte y controles bloqueados durante respawnDelay; solo entonces se
    /// destruye el controller y se crea uno nuevo. Sin este hueco,
    /// PlayerManager.Die() destruia y recreaba en la misma linea: no habia
    /// camara entre medias y la pantalla se quedaba en negro
    /// (mismo sintoma que F-54 en el CHANGELOG).
    ///
    /// El RPC de ocultar sale ANTES que el Instantiate del VFX: son dos
    /// mensajes de red independientes y en el cliente que muere no importa el
    /// orden porque ambos se ejecutan localmente al instante, pero en el
    /// resto de clientes cada uno llega por separado, y con el pato
    /// desapareciendo primero se ve "puf, plumas" en vez de "explosion...
    /// pato que sigue ahi... desaparece".
    /// </summary>
    private IEnumerator SecuenciaDeMuerte()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (shootinController != null)
        {
            shootinController.enabled = false;
        }

        PV.RPC(nameof(MostrarPato), RpcTarget.All, false);

        if (deathVfx != null)
        {
            PhotonNetwork.Instantiate(deathVfx.name, transform.position, transform.rotation);
        }

        if (playerPhotonSoundManager != null)
        {
            playerPhotonSoundManager.PlayDieSFX();
        }

        yield return new WaitForSeconds(respawnDelay);

        playerManager.Die();
    }

    [PunRPC]
    private void MostrarPato(bool visible)
    {
        for (int i = 0; i < renderersDelPato.Length; i++)
        {
            renderersDelPato[i].enabled = visible;
        }
    }
}
