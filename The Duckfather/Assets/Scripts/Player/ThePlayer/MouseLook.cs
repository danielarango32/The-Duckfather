using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


public class MouseLook : MonoBehaviour
{
    public float sensibility = 120f;


    [SerializeField] private Transform multiAimConstraint;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform camera;
    [SerializeField] private Transform cameraTarget;
    //float xRotation = 0f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        float sensX = Input.GetAxis("Mouse X") * sensibility * Time.deltaTime;
        float sensY = Input.GetAxis("Mouse Y") * sensibility * Time.deltaTime;



        /*     //FPS
        xRotation -= sensY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);  
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        */



        playerBody.Rotate(Vector3.up * sensX);
        multiAimConstraint.Rotate(Vector3.left * sensY);

        // camara
        camera.Rotate(Vector3.left * sensY);

        // camara target
        cameraTarget.Rotate(Vector3.left * sensY);

        

        cameraTarget.Rotate(Vector3.up * sensX);





    }
}
