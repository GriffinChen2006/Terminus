using UnityEngine;

public class AttackState : BaseState
{
    private readonly int animationHash;
    private readonly System.Action onFixedUpdate;

    public AttackState(PlayerController player, Animator animator, int animationHash, System.Action onFixedUpdate = null) 
        : base(player, animator)
    {
        this.animationHash = animationHash;
        this.onFixedUpdate = onFixedUpdate;
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(animationHash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
        player.Jump();
        player.HandleMovement();
        player.HandleAttacks();
        player.HandleAttackMomentum();

        onFixedUpdate?.Invoke();
    }
}