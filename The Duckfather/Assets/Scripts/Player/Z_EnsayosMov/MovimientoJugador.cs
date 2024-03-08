using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float speed;
    public float maxvelchange;


    private Vector2 input;
    private Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

    }

    private void FixedUpdate()
    {
        rb.AddForce(CalcularMovimiento(speed), ForceMode.VelocityChange);
    }
    Vector3 CalcularMovimiento(float _speed)
    {
        Vector3 targetvel = new Vector3(input.x, 0, input.y);
        targetvel = transform.TransformDirection(targetvel);

        targetvel *= _speed;

        Vector3 velocity = rb.velocity;



        if (input.magnitude > 0.5f)
        {
            Vector3 velochange = targetvel - velocity;

            velochange.x = Mathf.Clamp(velochange.x , -maxvelchange, maxvelchange);

            velochange.z = Mathf.Clamp(velochange.z, -maxvelchange, maxvelchange);

            velochange.y = 0;

            return velochange;
        }
        else
        {
            return new Vector3();
        }
    }

}
