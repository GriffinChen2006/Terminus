using UnityEngine;

public class GroundCombo2State : BaseState
{
    public GroundCombo2State(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(GroundCombo2Hash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}