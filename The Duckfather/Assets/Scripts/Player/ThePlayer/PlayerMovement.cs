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
    [SerializeField] private float CDDash;
    [SerializeField] private float dashModifier;
    private float dashPower = 1f;

    public bool canDash = true;

    //GRAVEDAD
    [SerializeField] float gravity = -20f;
    Vector3 velocity;


    //GROUND CHECK
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    public LayerMask groundeMask;
    [SerializeField] public bool isGrounded = false;
    
    private float dashTime;


    [Header("Globalizaci�n De Variables")]
    public float x, z;
    
    [Header("UI de dash")]
    [SerializeField] Slider sliderDash;
    
    private void Start()
    {
        dashTime = sliderDash.GetComponent<RectTransform>().sizeDelta.x;;
        sliderDash.maxValue = CDDash;
    }
    private void Update()
    {

        Movimiento();
        IsGrounded();
        Saltar();
        sliderDash.value = CDDash;

    }

    void Movimiento()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.F) && isGrounded && velocity.y < 0 && canDash)
        {
            dashPower = dashModifier;
            StartCoroutine(DashActivado());
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * dashPower * Time.deltaTime);
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
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(alturaSalto * -2 * gravity);
        }


        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator DashActivado()
    {
        yield return new WaitForSeconds(0.01f);
        dashPower = 1;
        canDash = false;
        
        yield return new WaitForSeconds(CDDash);
        canDash = true;
    }
    
}
