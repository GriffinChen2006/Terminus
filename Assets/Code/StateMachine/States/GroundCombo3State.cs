using UnityEngine;

public class GroundCombo3State : BaseState
{
    public GroundCombo3State(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(GroundCombo3Hash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}