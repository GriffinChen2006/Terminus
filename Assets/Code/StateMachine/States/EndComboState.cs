using UnityEngine;

public class EndComboState : BaseState
{
    public EndComboState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(EndComboHash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}