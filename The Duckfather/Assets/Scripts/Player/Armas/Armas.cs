using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armas : MonoBehaviour
{
    public bool inparent;

    private void Update()
    {
        if (!inparent)
        {
            VerificacionDeParent();
        }
        
    }


    private void Usable()
    {

    }

    private void VerificacionDeParent()
    {
        if (transform.parent != null)
        {
            Debug.Log("Este GameObject es hijo de otro");
            inparent = true;
        }
        else
        {
            Debug.Log("No parent");
            inparent = false;
        }
    }
}
