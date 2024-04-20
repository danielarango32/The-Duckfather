using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitadorFps : MonoBehaviour
{
    
    public int targetFPS = 60; // FPS objetivo
    private float deltaTime;

    void Awake()
    {
        // Asegurarse de que el valor de targetFPS no sea 0 o negativo
        targetFPS = Mathf.Max(targetFPS, 1);
        deltaTime = 1.0f / targetFPS;
    }

    void Update()
    {
        // Limitar los FPS
        float renderDelta = Time.smoothDeltaTime;
        float renderTime = 0.0f;

        while (renderTime < deltaTime)
        {
            renderTime += renderDelta;
        }
    }

}
