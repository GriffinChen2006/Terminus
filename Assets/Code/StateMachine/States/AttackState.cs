using UnityEngine;

public class AttackState : BaseState
{
    protected readonly int animationHash;
    protected readonly System.Action onFixedUpdate;

    public AttackState(PlayerController player, Animator animator, int animationHash, System.Action onFixedUpdate = null) 
        : base(player, animator)
    {
        this.animationHash = animationHash;
        this.onFixedUpdate = onFixedUpdate;
    }

    public override void OnEnter()
    {
        animator.CrossFadeInFixedTime(animationHash, crossFadeDuration, 0);
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

// Subclasses for each combo state
public class GroundCombo1State : AttackState
{
    public GroundCombo1State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("GroundCombo1")) { }
}

public class GroundCombo2State : AttackState
{
    public GroundCombo2State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("GroundCombo2")) { }
}

public class GroundCombo3State : AttackState
{
    public GroundCombo3State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("GroundCombo3")) { }
}

public class AirCombo1State : AttackState
{
    public AirCombo1State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("AirCombo1")) { }
}

public class AirCombo2State : AttackState
{
    public AirCombo2State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("AirCombo2")) { }
}

public class AirCombo3State : AttackState
{
    public AirCombo3State(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("AirCombo3")) { }
}

public class EndComboState : AttackState
{
    public EndComboState(PlayerController player, Animator animator) 
        : base(player, animator, Animator.StringToHash("EndCombo")) { }
}