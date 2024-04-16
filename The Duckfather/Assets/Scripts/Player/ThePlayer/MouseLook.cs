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
    [SerializeField] private Camera cameraC;
    //float xRotation = 0f;


    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        float sensX = Input.GetAxis("Mouse X");
        float sensY = Input.GetAxis("Mouse Y");



        /*     //FPS
        xRotation -= sensY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);  
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        */

        //modificación del aim constraint y el playerbody
        //playerBody.Rotate(Vector3.up * sensX);
        //multiAimConstraint.Rotate(Vector3.up * sensX);
        //multiAimConstraint.Rotate(Vector3.left * sensY);

        playerBody.Rotate(Vector3.up * sensX, Space.Self);
        multiAimConstraint.Rotate(Vector3.up * sensX, Space.Self);

        // camara
        //camera.Rotate(Vector3.left * sensY);

        // camara target
        Vector3 mousePos = Input.mousePosition;
        //Vector3 projectedPoint = cameraC.ScreenToWorldPoint(new Vector3(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"),8));
        Vector3 projectedPoint = cameraC.ScreenPointToRay(mousePos).GetPoint(8.0f);
        Debug.DrawLine(cameraC.transform.position, projectedPoint, Color.cyan, 0.3f);
        cameraTarget.position = projectedPoint;
        //cameraTarget.Rotate(Vector3.left * sensY);





    }
}
