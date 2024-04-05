using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Objeto base de los efectos de Power Up
/// </summary>
public abstract class PowerUpEffect : ScriptableObject
{
    public abstract void Apply(GameObject target);



}
