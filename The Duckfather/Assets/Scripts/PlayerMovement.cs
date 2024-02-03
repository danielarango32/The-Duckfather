using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //REFERENCIAS
    public CharacterController controller;


    //VELOCIDAD DEL JUGADOR
    [SerializeField] private float speed = 10f;
    [SerializeField] private float alturaSalto = 3f;


    //GRAVEDAD
    [SerializeField] float gravity = -9.77f;
    Vector3 velocity;


    //GROUND CHECK
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    public LayerMask groundeMask;
    [SerializeField] private bool isGrounded = false;

    


    private void Update()
    {
        //MOVIMIENTO
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);


        //GROUND CHECK
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundeMask);

        if (isGrounded && velocity.y <0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;


        //SALTO
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(alturaSalto * -2 * gravity);
        }


        controller.Move(velocity * Time.deltaTime);

    }

}
