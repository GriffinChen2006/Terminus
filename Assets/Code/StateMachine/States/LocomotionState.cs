using UnityEngine;

public class LocomotionState : BaseState
{
    public LocomotionState(PlayerController player, Animator animator) : base(player, animator)
    {
        
    }
    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(LocomotionHash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}