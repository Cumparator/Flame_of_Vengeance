using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки Движения")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Настройки Прыжка/Уворота")]
    [SerializeField] private float jumpForce = 8f; // Сила для 'прыжка' (может быть визуальным или коротким рывком)
    //[SerializeField] private float jumpDuration = 0.2f; если прыжок имеет длительность

    [Header("Настройки Переката/Рывка")]
    [SerializeField] private float rollSpeed = 10f;
    [SerializeField] private float rollDuration = 0.5f;

    public bool IsRolling { get; private set; }
    public Vector2 CurrentVelocity => _rb.linearVelocity;
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private PlayerAnimator _playerAnimator;
    private PlayerStamina _playerStamina;

    private bool _isJumping = false;
    private float _rollTimer = 0f;
    private Vector2 _rollDirection = Vector2.zero;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerStamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        FacingDirection = _playerInput.AimDirection;

        HandleRollStart();

        HandleJumpStart();

        UpdateRollState();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRollMovement();
    }
    private void HandleMovement()
    {
        switch (IsRolling)
        {
            case false when !_isJumping:
            {
                var moveDirection = _playerInput.MoveInput;
                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    moveDirection.Normalize();
                }

                var targetVelocity = moveDirection * moveSpeed;
                _rb.linearVelocity = new Vector2(targetVelocity.x, targetVelocity.y);
                break;
            }
            case false:
                _rb.linearVelocity = Vector2.zero;
                break;
        }
    }

     private void HandleJumpStart()
     {
         if (!_playerInput.JumpTriggered || !CanJump()) return;
         _isJumping = true; // Устанавливаем флаг
         _playerAnimator.TriggerJump(); // Вызываем метод аниматора
         _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
         // Временная неуязвимость? (логика в PlayerCombat)
         // Сброс флага _isJumping через время или по анимации/приземлению (если нужно)
         Invoke(nameof(StopJumping), 0.1f); // Пример: очень короткий прыжок
     }
     private void StopJumping()
     {
        _isJumping = false;
     }

    private void HandleRollStart()
    {
        if (!_playerInput.RollTriggered || !CanRoll()) return;
        if (!_playerStamina.TrySpendStamina(_playerStamina.rollStaminaCost)) return;
        IsRolling = true;
        _rollTimer = rollDuration;

        _rollDirection = _playerInput.MoveInput.sqrMagnitude > 0.1f 
            ? _playerInput.MoveInput.normalized 
            : _playerInput.AimDirection.normalized;

        _playerAnimator.TriggerRoll();
    }

    private void UpdateRollState()
    {
        if (!IsRolling) return;
        _rollTimer -= Time.deltaTime;
        if (_rollTimer <= 0f)
        {
            IsRolling = false;
        }
    }

     private void HandleRollMovement()
    {
        if (IsRolling)
        {
            _rb.linearVelocity = _rollDirection * rollSpeed;
        }
    }
     
    private bool CanMove()
    {
        return !IsRolling && !_isJumping;
    }

    private bool CanRoll()
    {
        return !IsRolling && !_isJumping && _playerStamina.HasEnoughStamina(_playerStamina.rollStaminaCost);
    }

     private bool CanJump()
    {
         return !IsRolling && !_isJumping;
    }
}