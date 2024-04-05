using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Destroyer : MonoBehaviour
{
    public float tiempo;


    private void Update()
    {
        tiempo = gameObject.GetComponent<ParticleSystem>().totalTime;
        destroyer();
    }

    void destroyer()
    {
        if (tiempo == gameObject.GetComponent<ParticleSystem>().main.duration)
        {
            Destroy(gameObject);
        }
    }

}
