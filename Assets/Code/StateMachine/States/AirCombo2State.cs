using UnityEngine;

public class AirCombo2State : BaseState
{
    public AirCombo2State(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(AirCombo2Hash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();
    }
}