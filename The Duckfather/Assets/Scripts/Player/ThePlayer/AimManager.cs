using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimManager : MonoBehaviour
{
    public GameObject cam;


    public float standarFov = 65f;
    public float aimFov = 40f;
    public float smoothInterFov = 10f;


    private void Update()
    {
        FieldOfViewLerp();
    }


    public void FieldOfViewLerp()
    {
        float currentFov = cam.GetComponent<Camera>().fieldOfView;

        float targetFov = Input.GetMouseButton(1) ? aimFov : standarFov;
        currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * smoothInterFov);

        cam.GetComponent<Camera>().fieldOfView = currentFov;
    }

}
