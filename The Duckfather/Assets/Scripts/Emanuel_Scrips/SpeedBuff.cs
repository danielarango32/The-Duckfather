using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Codigo del Scripteable del power up de velocidad
/// </summary>
[CreateAssetMenu(menuName = "PowerUps/SpeedBuff")] //Para poder crear el menu en el explorer
public class SpeedBuff : PowerUpEffect
{
  
    public float speedAmount;  //Valor a sumar con el Power Up

    [SerializeField]
    private float speedDuration;  //Duracion del Power Up


    public override void Apply(GameObject target)
    {
         target.GetComponent<PlayerMovement>().SetMoveSpeed(speedAmount, speedDuration);  //Eniva los componentes al PlayerMovement para que accedan a los componentes privados e inicien la Corutina

    }
    
  
}
