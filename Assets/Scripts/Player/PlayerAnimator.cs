using UnityEngine;

// Требуем наличия всех необходимых компонентов
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerAnimator : MonoBehaviour
{
    // --- Компоненты --- 
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;

    // --- Хеши Параметров Аниматора (для производительности) ---
    private readonly int _hashSpeed = Animator.StringToHash("Speed"); 
    // Убираем MoveX/MoveY для Blend Tree, но оставим для флипа
    // private readonly int _hashMoveX = Animator.StringToHash("MoveX"); 
    // private readonly int _hashMoveY = Animator.StringToHash("MoveY");
    private readonly int _hashIsRolling = Animator.StringToHash("IsRolling");
    private readonly int _hashIsBlocking = Animator.StringToHash("IsBlocking");
    private readonly int _hashJumpTrigger = Animator.StringToHash("Jump");
    private readonly int _hashRollTrigger = Animator.StringToHash("Roll");
    // Заменяем общий MeleeAttack на триггеры для комбо
    // private readonly int _hashMeleeAttackTrigger = Animator.StringToHash("MeleeAttack");
    private readonly int _hashAttack1Trigger = Animator.StringToHash("Attack1");
    private readonly int _hashAttack2Trigger = Animator.StringToHash("Attack2");
    private readonly int _hashAttack3Trigger = Animator.StringToHash("Attack3");
    private readonly int _hashRangedAttackTrigger = Animator.StringToHash("RangedAttack");
    private readonly int _hashHurtTrigger = Animator.StringToHash("Hurt");
    private readonly int _hashDeathTrigger = Animator.StringToHash("Death");

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        // Получаем ссылки на все компоненты
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

    // --- Обновление Параметров Аниматора --- 

    private void UpdateMovementAnimation()
    {
        // Обновляем только Speed для 1D Blend Tree
        float currentSpeed = new Vector2(_playerMovement.CurrentVelocity.x, _playerMovement.CurrentVelocity.y).magnitude;
        _animator.SetFloat(_hashSpeed, currentSpeed);

        // Убираем установку MoveX/MoveY для Blend Tree
        // Vector2 facingDirection = _playerMovement.FacingDirection;
        // _animator.SetFloat(_hashMoveX, facingDirection.x);
        // _animator.SetFloat(_hashMoveY, facingDirection.y);
    }

    private void UpdateStateAnimation()
    {
        _animator.SetBool(_hashIsRolling, _playerMovement.IsRolling);
        _animator.SetBool(_hashIsBlocking, _playerCombat.IsBlocking);
    }

    private void HandleSpriteFlip()
    {
        // Поворачиваем спрайт влево/вправо в зависимости от направления взгляда
        // Используем горизонтальную составляющую направления взгляда
        float horizontalDirection = _playerMovement.FacingDirection.x;
        
        // Поворачиваем, если смотрим влево (отрицательный X)
        if (Mathf.Abs(horizontalDirection) > 0.01f) // Проверка, чтобы не флипать при нулевом X
        {
             _spriteRenderer.flipX = horizontalDirection < 0;
        }
        // Не делаем ничего, если X близок к нулю (например, смотрим строго вверх/вниз)
    }

    // --- Публичные Методы для Запуска Анимаций --- 
    // Теперь PlayerCombat будет вызывать эти методы

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
                _animator.SetTrigger(_hashAttack1Trigger); // По умолчанию играем первую
                break;
        }
    }

    public void TriggerRangedAttack()
    {
        _animator.SetTrigger(_hashRangedAttackTrigger);
    }

    // Добавим методы для Hurt и Death для единообразия
    public void TriggerHurt()
    {
        _animator.SetTrigger(_hashHurtTrigger);
    }

    public void TriggerDeath()
    {
        _animator.SetTrigger(_hashDeathTrigger);
        // Возможно, здесь стоит отключить другие компоненты (Input, Movement, Combat)
        // чтобы предотвратить дальнейшие действия после смерти
        // _playerInput.enabled = false;
        // _playerMovement.enabled = false;
        // _playerCombat.enabled = false;
    }

    // Добавим метод для обновления состояния блока
    public void SetBlocking(bool isBlocking)
    {
        _animator.SetBool(_hashIsBlocking, isBlocking);
    }
}
