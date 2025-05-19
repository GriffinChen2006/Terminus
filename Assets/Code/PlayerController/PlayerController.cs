using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;

    public PlayerAttackStats AttackStats;

    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;
    
    // movement vars
    private Vector2 _moveVelocity;
    private bool _isFacingRight;
    
    // collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;
    
    // jump vars
    public float VerticalVelocity {get; private set;}
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;
    
    // apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;
    
    // jump buffer vars

    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;
    
    // coyote time vars

    private float _coyotetimer;
    
    // attack vars
    
    private bool _recovering;
    private bool _isAttacking;
    private float _attackStateChangeWindow;
    private float _attackTimer;
    private float _comboTimer;
    private int _combo;
    
    // attack momentum vars
    
    private float _speedBoost;
    private Vector2 _boostDirection;
    private bool _boostActivatedInAir;
    
    // state machine
    
    private StateMachine StateMachine;
    
    // animator
    
    public Animator Animator;
    
    private void Awake()
    {
        _isFacingRight = true;
        _isAttacking = false;
        _recovering = false;

        _combo = 0;
        
        _rb = GetComponent<Rigidbody2D>();
        
        StateMachine = new StateMachine();
        
        // states
        
        var locomotionState = new LocomotionState(this, Animator);
        var jumpState = new JumpState(this, Animator);
        var groundCombo1State = new AttackState(this, Animator, Animator.StringToHash("groundCombo1"));
        var groundCombo2State = new AttackState(this, Animator, Animator.StringToHash("groundCombo2"));
        var groundCombo3State = new AttackState(this, Animator, Animator.StringToHash("groundCombo3"));
        var airCombo1State = new AttackState(this, Animator, Animator.StringToHash("airCombo1"));
        var airCombo2State = new AttackState(this, Animator, Animator.StringToHash("airCombo2"));
        var airCombo3State = new AttackState(this, Animator, Animator.StringToHash("airCombo3"));
        var endComboState = new AttackState(this, Animator, Animator.StringToHash("endCombo"));
        
        // transitions
        
        At(locomotionState, jumpState, new FuncPredicate(() => _isJumping));
        Any(locomotionState, new FuncPredicate(() => !_isJumping && !_isAttacking && !_recovering));
        
        Any(groundCombo1State, new FuncPredicate(() => _attackStateChangeWindow > 0 && _isGrounded && _combo == 1));
        Any(groundCombo2State, new FuncPredicate(() => _attackStateChangeWindow > 0 && _isGrounded && _combo == 2));
        Any(groundCombo3State, new FuncPredicate(() => _attackStateChangeWindow > 0 && _isGrounded && _combo == 3));
        Any(airCombo1State, new FuncPredicate(() => _attackStateChangeWindow > 0 && !_isGrounded && _combo == 1));
        Any(airCombo2State, new FuncPredicate(() => _attackStateChangeWindow > 0 && !_isGrounded && _combo == 2));
        Any(airCombo3State, new FuncPredicate(() => _attackStateChangeWindow > 0 && !_isGrounded && _combo == 3));
        Any(endComboState, new FuncPredicate(() => _recovering));
        
        At(endComboState, locomotionState, new FuncPredicate(() => _recovering));
        
        StateMachine.SetState(locomotionState);
    }
    
    void At(IState from, IState to, IPredicate condition) => StateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => StateMachine.AddAnyTransition(to, condition);
    
    private void Update()
    {
        JumpChecks();
        CountTimers();
        StateMachine.Update();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        StateMachine.FixedUpdate();
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            if (!_isAttacking)
            {
                TurnCheck(moveInput); 
            }
            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }
            
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.deltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.deltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        } 
    }

    public void HandleMovement()
    {
        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
    }

    public void HandleAttackMomentum()
    {
        if (_speedBoost > 0)
        {
            _rb.linearVelocity += _boostDirection * _speedBoost;
            _speedBoost -= (_isGrounded ? AttackStats.groundBoostDecayRate : AttackStats.airBoostDecayRate) * Time.deltaTime;
            _speedBoost = Mathf.Max(_speedBoost, 0);
        }
    }
    
    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (_isFacingRight != turnRight)
        {
            _isFacingRight = turnRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength,
            MoveStats.GroundLayer);
        
        if (_groundHit.collider)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }
    
    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        if (_isGrounded && _boostActivatedInAir)
        {
            CancelAirBoost();
        }
    }
    
    private void CancelAirBoost()
    {
        _speedBoost = 0;
        _boostActivatedInAir = false;
    }

    private void JumpChecks()
    {
        if (InputManager.JumpPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTIme;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.JumpReleased)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyotetimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }
    public void Jump()
    {
        if (_isJumping)
        {
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }
            if (VerticalVelocity >= 0f)
            {
                if (_speedBoost <= 0 && !_isAttacking)
                {
                    _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                    if (_apexPoint > MoveStats.ApexThreshold)
                    {
                        if (!_isPastApexThreshold)
                        {
                            _isPastApexThreshold = true;
                            _timePastApexThreshold = 0f;
                        }

                        if (_isPastApexThreshold)
                        {
                            _timePastApexThreshold += Time.fixedDeltaTime;
                            if (_timePastApexThreshold < MoveStats.ApexHangTime)
                            {
                                VerticalVelocity = 0f;
                            }
                            else
                            {
                                VerticalVelocity = -0.01f;
                            }
                        }
                    }
                    else
                    {
                        VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                        if (_isPastApexThreshold)
                        {
                            _isPastApexThreshold = false;
                        }
                    }
                }
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                }
            }
            
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f,
                    (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    private void CountTimers()
    { 
        if (!_isAttacking)
        { 
            _comboTimer -= Time.deltaTime;
        }
        _attackStateChangeWindow -= Time.deltaTime;
        _attackTimer -= Time.deltaTime;
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded)
        {
            _coyotetimer -= Time.deltaTime;
        }
        else
        {
            _coyotetimer = MoveStats.JumpCoyoteTime;
        }
    }

    private void InitiateAttack(int combo)
    {
        if (!_isAttacking)
        {
            _isAttacking = true;
        }
        _combo = combo;
        _comboTimer = AttackStats.comboPersistenceDuration;
        _attackTimer = AttackStats.getCD(_isGrounded, combo - 1);
        _attackStateChangeWindow = 0.01f;
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;
        
        float distanceToMouse = Vector2.Distance(mousePosition, transform.position);
        
        if (distanceToMouse > AttackStats.minMouseDistance)
        {
            float boostStrength = _isGrounded ? AttackStats.groundBoostStrength : AttackStats.airBoostStrength;
            _speedBoost = Mathf.Clamp(distanceToMouse / AttackStats.maxMouseDistance, 0.5f, 1f) * boostStrength;
        }
        
        else
        {
            _speedBoost = 0f;
        }

        if (_isGrounded)
        {
            if ((_isFacingRight && directionToMouse.x > 0) || (!_isFacingRight && directionToMouse.x < 0))
            {
                _boostDirection = new Vector2(Mathf.Sign(directionToMouse.x), 0f);

                if (_boostDirection.x * _rb.linearVelocity.x < 0)
                {
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y * 0.5f);
                }

                _boostActivatedInAir = false;
            }
            else
            {
                _speedBoost = 0f;
            }
        }
        else if ((_isFacingRight && directionToMouse.x > 0) || (!_isFacingRight && directionToMouse.x < 0))
        {
            _boostDirection = new Vector2(directionToMouse.x, directionToMouse.y).normalized;

            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _rb.linearVelocity.y - Vector2.Dot(_rb.linearVelocity, -_boostDirection) * -_boostDirection.y
            );

            _boostActivatedInAir = true;
        }
        else
        {
            _boostDirection = new Vector2(0f, directionToMouse.y).normalized;
            _boostActivatedInAir = true;
        }
        if (_isFacingRight != directionToMouse.x > 0)
        {
            _isFacingRight = directionToMouse.x > 0;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    public void HandleAttacks()
    {
        if (_attackTimer <= 0)
        {
            _isAttacking = false;
            if (_recovering)
            {
                _recovering = false;
                _combo = 0;
            }
        }

        if (_comboTimer <= 0 && _combo != 0)
        {
            _combo = 0;
        }
        if (_combo == AttackStats.comboLength && !_recovering && !_isAttacking)
        {
            _attackTimer = AttackStats.getCD(_isGrounded, _combo);
            _recovering = true;
        }
        if (InputManager.AttackPressed && !_isAttacking && !_recovering)
        {
            InitiateAttack(_combo + 1);
        }
    }
}
