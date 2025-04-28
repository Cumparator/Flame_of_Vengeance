using Player;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;

    // --- Хеши Параметров Аниматора (для производительности) ---
    private readonly int _hashSpeed = Animator.StringToHash("Speed"); 
    private readonly int _hashIsRolling = Animator.StringToHash("IsRolling");
    private readonly int _hashIsBlocking = Animator.StringToHash("IsBlocking");
    private readonly int _hashJumpTrigger = Animator.StringToHash("Jump");
    private readonly int _hashRollTrigger = Animator.StringToHash("Roll");
    private readonly int _hashAttack1Trigger = Animator.StringToHash("Attack1");
    private readonly int _hashAttack2Trigger = Animator.StringToHash("Attack2");
    private readonly int _hashAttack3Trigger = Animator.StringToHash("Attack3");
    private readonly int _hashRangedAttackTrigger = Animator.StringToHash("RangedAttack");
    private readonly int _hashHurtTrigger = Animator.StringToHash("Hurt");
    private readonly int _hashDeathTrigger = Animator.StringToHash("Death");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        UpdateMovementAnimation();
        UpdateStateAnimation();
        HandleSpriteFlip();
    }

    private void UpdateMovementAnimation()
    {
        var currentSpeed = new Vector2(_playerMovement.CurrentVelocity.x, _playerMovement.CurrentVelocity.y).magnitude;
        _animator.SetFloat(_hashSpeed, currentSpeed);
    }

    private void UpdateStateAnimation()
    {
        _animator.SetBool(_hashIsRolling, _playerMovement.IsRolling);
        _animator.SetBool(_hashIsBlocking, _playerCombat.IsBlocking);
    }

    private void HandleSpriteFlip()
    {
        var horizontalDirection = _playerMovement.FacingDirection.x;
        
        if (Mathf.Abs(horizontalDirection) > 0.01f)
        {
             _spriteRenderer.flipX = horizontalDirection < 0;
        }
    }

    public void TriggerJump()
    {
        _animator.SetTrigger(_hashJumpTrigger);
    }
    
    public void TriggerRoll()
    {
         _animator.SetTrigger(_hashRollTrigger);
    }

    public void TriggerMeleeAttack(int attackNumber)
    {
        switch (attackNumber)
        {
            case 1: _animator.SetTrigger(_hashAttack1Trigger); break;
            case 2: _animator.SetTrigger(_hashAttack2Trigger); break;
            case 3: _animator.SetTrigger(_hashAttack3Trigger); break;
            default: 
                Debug.LogWarning($"Invalid attack number: {attackNumber}"); 
                _animator.SetTrigger(_hashAttack1Trigger);
                break;
        }
    }

    public void TriggerRangedAttack()
    {
        _animator.SetTrigger(_hashRangedAttackTrigger);
    }

    public void TriggerHurt()
    {
        _animator.SetTrigger(_hashHurtTrigger);
    }

    public void TriggerDeath()
    {
        _animator.SetTrigger(_hashDeathTrigger);
    }

    public void SetBlocking(bool isBlocking)
    {
        _animator.SetBool(_hashIsBlocking, isBlocking);
    }
}