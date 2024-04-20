using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimState : AimBaseState
{
    public override void EnterState(MouseLook aim)
    {
        aim.anim.SetBool("IsAiming", true);
        aim.currentFov = aim.adsFov;
    }

    public override void UpdateState(MouseLook aim)
    {
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            aim.SwitchState(aim.Hip);
        }
    }
}
