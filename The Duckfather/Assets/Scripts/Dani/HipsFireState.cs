using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipsFireState : AimBaseState
{
    public override void EnterState(MouseLook aim)
    {
        aim.anim.SetBool("IsAiming", false);
        aim.currentFov = aim.hipsFov;   
    }

    public override void UpdateState(MouseLook aim)
    {
        if(Input.GetKey(KeyCode.Mouse1))
        {
            aim.SwitchState(aim.Aim);
        }
    }
}
