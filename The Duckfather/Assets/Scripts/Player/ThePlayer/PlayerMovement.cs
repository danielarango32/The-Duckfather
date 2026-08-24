using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks
{

    //REFERENCIAS
    public CharacterController controller;


    //VELOCIDAD DEL JUGADOR
    [SerializeField] private float speed = 10f;
    [SerializeField] private float alturaSalto = 3f;


    //DASH
    [Tooltip("Segundos de enfriamiento entre dos dashes")]
    [SerializeField] private float CDDash = 10f;

    [Tooltip("Multiplicador de velocidad mientras dura el impulso")]
    [SerializeField] private float dashModifier = 4f;

    [Tooltip("Duracion del impulso en segundos")]
    [SerializeField] private float duracionDash = 0.15f;

    private float dashPower = 1f;

    public bool canDash = true;

    [Tooltip("Objeto UI/Barra/dash; de aqui se saca el slider de enfriamiento")]
    public RectTransform DashBar;

    // El campo sliderDash del prefab apuntaba a un TextMeshProUGUI (el contador
    // de balas), asi que Unity lo dejaba en null: Start() reventaba en su
    // primera linea y Update() lanzaba una NullReferenceException por frame.
    // Ahora el slider se resuelve desde DashBar, que si apunta a UI/Barra/dash.
    private Slider sliderDash;

    private float dashCooldownRestante;

    //GRAVEDAD
    [SerializeField] float gravity = -20f;
    Vector3 velocity;


    //GROUND CHECK
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    public LayerMask groundeMask;
    [SerializeField] public bool isGrounded = false;


    [Header("Globalizacion De Variables")]
    public float x, z;

    // Ni PV ni playerManager llegaron a usarse nunca: la red se consulta con la
    // propiedad photonView que ya trae MonoBehaviourPunCallbacks.
    private PlayerPhotonSoundManager playerPhotonSoundManager;

    // Sincronizacion por red. Antes salia un RPC de movimiento y otro de salto
    // en cada frame, cambiara algo o no.
    private const float MovementSyncThreshold = 0.05f;
    private const float MovementSyncInterval = 0.1f;

    private float lastSentX;
    private float lastSentZ;
    private float nextMovementSyncTime;
    private bool lastSentJumpState;

    private void Start()
    {
        if (DashBar != null)
        {
            sliderDash = DashBar.GetComponent<Slider>();
        }

        if (sliderDash != null)
        {
            sliderDash.minValue = 0f;
            sliderDash.maxValue = 1f;
            sliderDash.value = 1f;
        }

        playerPhotonSoundManager = GetComponent<PlayerPhotonSoundManager>();

    }
    private void Update()
    {

        Movimiento();
        IsGrounded();
        Saltar();
        ActualizarEnfriamientoDash();


    }

    [PunRPC]
    public void SyncMovement(float x, float z)
    {
        this.x = x;
        this.z = z;
    }


    void Movimiento()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        SyncMovementIfChanged();

        // Sobraba comprobar velocity.y < 0: IsGrounded() deja la velocidad
        // vertical en -2 en cuanto se toca el suelo, y ademas se evaluaba con el
        // valor del frame anterior porque IsGrounded() corre despues de esto.
        if (Input.GetKeyDown(KeyCode.F) && isGrounded && canDash)
        {
            StartCoroutine(DashActivado());
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * dashPower * Time.deltaTime);
    }

    /// <summary>
    /// Envia el input de movimiento solo cuando cambia de verdad, y como mucho
    /// 1/MovementSyncInterval veces por segundo.
    /// </summary>
    private void SyncMovementIfChanged()
    {
        if (!photonView.IsMine || Time.time < nextMovementSyncTime)
        {
            return;
        }

        if (!MovementNeedsSync())
        {
            return;
        }

        lastSentX = x;
        lastSentZ = z;
        nextMovementSyncTime = Time.time + MovementSyncInterval;
        photonView.RPC(nameof(SyncMovement), RpcTarget.Others, x, z);
    }

    private bool MovementNeedsSync()
    {
        // Arrancar y pararse se notifican siempre: con solo el umbral, el
        // ultimo valor enviado podria quedarse en un residuo y el muneco
        // remoto seguiria andando en el sitio.
        bool stoppedNow = x == 0f && z == 0f;
        bool wasStopped = lastSentX == 0f && lastSentZ == 0f;

        if (stoppedNow != wasStopped)
        {
            return true;
        }

        return Mathf.Abs(x - lastSentX) > MovementSyncThreshold
            || Mathf.Abs(z - lastSentZ) > MovementSyncThreshold;
    }

    void IsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundeMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    void Saltar()
    {
        bool isJumping = Input.GetButtonDown("Jump") && isGrounded;

        if (isJumping)
        {
            velocity.y = Mathf.Sqrt(alturaSalto * -2 * gravity);
        }

        SyncJumpStateIfChanged(isJumping);

        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// La rama else mandaba SyncJumpState(false) en cada frame. Ahora solo se
    /// envia en el flanco: dos RPC por salto en lugar de 60 por segundo.
    /// </summary>
    private void SyncJumpStateIfChanged(bool isJumping)
    {
        if (!photonView.IsMine || isJumping == lastSentJumpState)
        {
            return;
        }

        lastSentJumpState = isJumping;
        // SyncJumpState es un [PunRPC] de AnimatorController, en este mismo
        // GameObject: por eso va por nombre y no con nameof.
        photonView.RPC("SyncJumpState", RpcTarget.Others, isJumping);
    }

    /// <summary>
    /// Impulso de duracion fija. Antes esperaba 0,01 s -menos de un frame a
    /// 60 fps- y ponia canDash a false despues del yield, asi que el dash duraba
    /// un frame y el bloqueo llegaba tarde.
    ///
    /// No hace falta RPC: el PhotonView del pato observa un PhotonTransformView,
    /// asi que el desplazamiento ya viaja a los demas clientes.
    /// </summary>
    IEnumerator DashActivado()
    {
        canDash = false;
        dashCooldownRestante = CDDash;
        dashPower = dashModifier;

        yield return new WaitForSeconds(duracionDash);

        dashPower = 1f;
    }

    /// <summary>
    /// Descuenta el enfriamiento y lo refleja en la barra: 0 recien gastado,
    /// 1 listo para volver a usarse. Antes se le asignaba Time.time, que crece
    /// sin limite y dejaba la barra clavada al maximo.
    /// </summary>
    private void ActualizarEnfriamientoDash()
    {
        if (dashCooldownRestante > 0f)
        {
            dashCooldownRestante = Mathf.Max(0f, dashCooldownRestante - Time.deltaTime);
            canDash = dashCooldownRestante <= 0f;
        }

        if (sliderDash == null)
        {
            return;
        }

        sliderDash.value = CDDash > 0f
            ? 1f - (dashCooldownRestante / CDDash)
            : 1f;
    }


    //Seccion de PowerUps
    //PowerUP de Velocidad
    public void SetMoveSpeed(float newSpeedAdjustment, float returnTime)
    {
        speed += newSpeedAdjustment;
        playerPhotonSoundManager.PlayPower1SFX();
        StartCoroutine(ReturnSpeed(newSpeedAdjustment, returnTime));
    }
    public IEnumerator ReturnSpeed(float newSpeedAdjustment, float returnTime)
    {



        yield return new WaitForSeconds(returnTime);
        ReturnMoveSpeed(newSpeedAdjustment);

    }

    public void ReturnMoveSpeed(float newSpeedAdjustment)
    {
        speed -= newSpeedAdjustment;
    }

    //PowerUp de Salto

    public void SetJumpAmount(float newJumpAdjustment, float returnTime)
    {
        alturaSalto += newJumpAdjustment;
        playerPhotonSoundManager.PlayPower2SFX();
        StartCoroutine(ReturnJumpTime(newJumpAdjustment, returnTime));
    }
    public IEnumerator ReturnJumpTime(float newJumpAdjustment, float returnTime)
    {



        yield return new WaitForSeconds(returnTime);
        ReturnJump(newJumpAdjustment);

    }

    public void ReturnJump(float newJumpAdjustment)
    {
        alturaSalto -= newJumpAdjustment;
    }
}
