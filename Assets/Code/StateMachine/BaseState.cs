using UnityEngine;

public abstract class BaseState : IState
{
    protected readonly PlayerController player;
    protected readonly Animator animator;
    
    protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    protected static readonly int JumpHash = Animator.StringToHash("Jump");
    protected static readonly int GroundCombo1Hash = Animator.StringToHash("GroundCombo1");
    protected static readonly int GroundCombo2Hash = Animator.StringToHash("GroundCombo2");
    protected static readonly int GroundCombo3Hash = Animator.StringToHash("GroundCombo3");
    protected static readonly int AirCombo1Hash = Animator.StringToHash("AirCombo1");
    protected static readonly int AirCombo2Hash  = Animator.StringToHash("AirCombo2");
    protected static readonly int AirCombo3Hash  = Animator.StringToHash("AirCombo3");
    protected static readonly int EndComboHash = Animator.StringToHash("EndCombo");

    protected const float crossFadeDuration = 0.1f;

    protected BaseState(PlayerController player, Animator animator)
    {
        this.player = player;
        this.animator = animator;
    }
    
    public virtual void OnEnter()
    {
      
    }

    public virtual void OnExit()
    {
      
    }

    public virtual void Update()
    {
      
    }

    public virtual void FixedUpdate()
    {
      
    }
}