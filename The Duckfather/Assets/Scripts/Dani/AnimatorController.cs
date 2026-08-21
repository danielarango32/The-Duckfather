using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    // Lo escriben dos caminos distintos (el dueno y el RPC): conviene que sea
    // literalmente el mismo parametro.
    private const string IsJumpingParameter = "IsJumping";

    [SerializeField] private Animator animator;
    private PlayerMovement playerMovement;
    private PhotonView photonView;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        // x y z los alimenta el input local en el dueno y el RPC SyncMovement
        // en los remotos, asi que sirven para los dos casos.
        animator.SetFloat("MotionX", playerMovement.x);
        animator.SetFloat("MotionY", playerMovement.z);

        UpdateOwnJumpState();
    }

    /// <summary>
    /// El salto solo lo decide el dueno del pato. Este bloque corria tambien en
    /// los jugadores remotos: leia el input LOCAL, con lo que todos los patos
    /// saltaban a la vez, y ademas pisaba en cada frame el valor que acababa de
    /// escribir SyncJumpState, dejando ese RPC sin ningun efecto.
    /// </summary>
    private void UpdateOwnJumpState()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }

        animator.SetBool(IsJumpingParameter,
            Input.GetButtonDown("Jump") && playerMovement.isGrounded);
    }

    [PunRPC]
    public void SyncJumpState(bool isJumping)
    {
        animator.SetBool(IsJumpingParameter, isJumping);
    }
}
