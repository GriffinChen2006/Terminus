using UnityEngine;

public class GroundCombo1State : BaseState
{
    public GroundCombo1State(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(GroundCombo1Hash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}