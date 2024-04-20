using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse look con cinemachine")]
    //public Cinemachine.AxisState xAxis, yAxis;
    [SerializeField] Transform cameraFollowPos;
    [SerializeField] float mouseSense = 1f;
    float xAxis, yAxis;

    [HideInInspector] public Animator anim;  

    [HideInInspector] public CinemachineVirtualCamera vCam;
    public float adsFov = 40;
    [HideInInspector] public float hipsFov;
    [HideInInspector] public float currentFov;
    public float fovSmothSpeed = 10 ;

    AimBaseState currentState;
    public HipsFireState Hip = new HipsFireState();
    public AimState Aim = new AimState();

    [SerializeField] Transform aimPos;
    [SerializeField] float aimSmothSpeed = 10;
    [SerializeField] LayerMask aimMask;


    [Header("Otras cosas    ")]
    public float sensibility = 120f;

    

    [SerializeField] private Transform multiAimConstraint;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform cameraN;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Camera cameraC;
    //float xRotation = 0f;


    private void Start()
    {
        vCam = GetComponentInChildren<CinemachineVirtualCamera>();
        hipsFov = vCam.m_Lens.FieldOfView;

        anim = GetComponentInChildren<Animator>();  
        SwitchState(Hip);
        //Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        //float sensX = Input.GetAxis("Mouse X");
        //float sensY = Input.GetAxis("Mouse Y");



        ///*     //FPS
        //xRotation -= sensY;
        //xRotation = Mathf.Clamp(xRotation, -80f, 80f);  
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //*/

        ////modificación del aim constraint y el playerbody
        ////playerBody.Rotate(Vector3.up * sensX);
        ////multiAimConstraint.Rotate(Vector3.up * sensX);
        ////multiAimConstraint.Rotate(Vector3.left * sensY);

        //playerBody.Rotate(Vector3.up * sensX, Space.Self);
        //multiAimConstraint.Rotate(Vector3.up * sensX, Space.Self);

        //// camara
        ////cameraN.Rotate(Vector3.left * sensY);

        //// camara target
        //Vector3 mousePos = Input.mousePosition;
        ////Vector3 projectedPoint = cameraC.ScreenToWorldPoint(new Vector3(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"),8));
        //Vector3 projectedPoint = cameraC.ScreenPointToRay(mousePos).GetPoint(8.0f);
        //Debug.DrawLine(cameraC.transform.position, projectedPoint, Color.cyan, 0.3f);
        //cameraTarget.position = projectedPoint;
        ////cameraTarget.Rotate(Vector3.left * sensY);

        //virtual camera
        //xAxis.Update(Time.deltaTime);
        //yAxis.Update(Time.deltaTime);

        xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
        yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
        //yAxis = Mathf.Clamp(yAxis,-80, 80);

        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, currentFov, fovSmothSpeed * Time.deltaTime);

        Vector2 screenCenter = new Vector2 (Screen.width/2, Screen.height/2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmothSpeed* Time.deltaTime);
        }

        currentState.UpdateState(this);
    }

    private void LateUpdate()
    {
        cameraFollowPos.localEulerAngles = new Vector3(yAxis, cameraFollowPos.localEulerAngles.y, cameraFollowPos.localEulerAngles.z);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);   
    }

    public void SwitchState(AimBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }
}
