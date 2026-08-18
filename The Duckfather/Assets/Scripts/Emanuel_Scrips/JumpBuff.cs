using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Codigo del Scripteable del power up de salto
/// </summary>
[CreateAssetMenu(menuName = "PowerUps/JumpBuff")] //Para poder crear el menu en el explorer
public class JumpBuff : PowerUpEffect
{

    public float jumpAmount;  //Valor a sumar con el Power Up

    [SerializeField]
    private float jumpDuration;  //Duracion del Power Up


    public override void Apply(GameObject target)
    {
        target.GetComponent<PlayerMovement>().SetJumpAmount(jumpAmount, jumpDuration);  //Eniva los componentes al PlayerMovement para que accedan a los componentes privados e inicien la Corutina

    }

}
