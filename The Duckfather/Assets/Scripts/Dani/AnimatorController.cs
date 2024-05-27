using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private PlayerMovement playerMovement;

        
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
    }

    private void Update()
    {
        animator.SetFloat("MotionX",playerMovement.x);
        animator.SetFloat("MotionY", playerMovement.z);
        if (Input.GetButtonDown("Jump") && playerMovement.isGrounded)
        {
            animator.SetBool("IsJumping", true);
        }
        else
        {
            animator.SetBool("IsJumping", false);
        }
    }

    [PunRPC]
    public void SyncJumpState(bool isJumping)
    {
        animator.SetBool("IsJumping", isJumping);
    }
}
