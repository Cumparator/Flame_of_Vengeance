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
    // Используем Animator.StringToHash для избежания сравнения строк в Update
    private readonly int _hashSpeed = Animator.StringToHash("Speed");
    private readonly int _hashMoveX = Animator.StringToHash("MoveX");
    private readonly int _hashMoveY = Animator.StringToHash("MoveY");
    private readonly int _hashIsRolling = Animator.StringToHash("IsRolling");
    private readonly int _hashIsBlocking = Animator.StringToHash("IsBlocking");
    private readonly int _hashJumpTrigger = Animator.StringToHash("Jump"); // Триггер для прыжка
    private readonly int _hashRollTrigger = Animator.StringToHash("Roll"); // Триггер для переката
    private readonly int _hashMeleeAttackTrigger = Animator.StringToHash("MeleeAttack");
    private readonly int _hashRangedAttackTrigger = Animator.StringToHash("RangedAttack");
    private readonly int _hashHurtTrigger = Animator.StringToHash("Hurt");
    private readonly int _hashDeathTrigger = Animator.StringToHash("Death");
    // Добавь хеши для других параметров, если они есть в твоем Animator Controller

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
        // Обновляем параметры аниматора каждый кадр
        UpdateMovementAnimation();
        UpdateStateAnimation();
        UpdateActionTriggers();
        HandleSpriteFlip();
    }

    // --- Обновление Параметров Аниматора --- 

    private void UpdateMovementAnimation()
    {
        // Получаем текущую скорость движения (только по X/Y)
        float currentSpeed = new Vector2(_playerMovement.CurrentVelocity.x, _playerMovement.CurrentVelocity.y).magnitude;
        _animator.SetFloat(_hashSpeed, currentSpeed);

        // Передаем направление взгляда/прицеливания для Blend Tree
        // Это позволит анимации правильно отображать направление (вверх, вниз, влево, вправо и диагонали)
        Vector2 facingDirection = _playerMovement.FacingDirection; // Берем из PlayerMovement
        _animator.SetFloat(_hashMoveX, facingDirection.x);
        _animator.SetFloat(_hashMoveY, facingDirection.y);
    }

    private void UpdateStateAnimation()
    {
        // Передаем состояния
        _animator.SetBool(_hashIsRolling, _playerMovement.IsRolling);
        _animator.SetBool(_hashIsBlocking, _playerCombat.IsBlocking);
        // _animator.SetBool(_hashIsJumping, _playerMovement.IsJumping); // Если прыжок - это состояние
    }

    private void UpdateActionTriggers()
    {
        // Проверяем триггеры действий из PlayerInput и активируем анимации
        // Триггеры сбрасываются в PlayerInput в LateUpdate, так что сработают один раз
        if (_playerInput.JumpTriggered)
        {
            _animator.SetTrigger(_hashJumpTrigger);
        }
        if (_playerInput.RollTriggered)
        {
            _animator.SetTrigger(_hashRollTrigger);
        }
        if (_playerInput.MeleeTriggered)
        {
            _animator.SetTrigger(_hashMeleeAttackTrigger);
        }
        if (_playerInput.RangedTriggered)
        {
            _animator.SetTrigger(_hashRangedAttackTrigger);
        }
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

    // --- Публичные Методы для Внешних Событий (например, получение урона) ---

    public void TriggerHurtAnimation()
    {
        _animator.SetTrigger(_hashHurtTrigger);
    }

    public void TriggerDeathAnimation()
    {
        _animator.SetTrigger(_hashDeathTrigger);
        // Возможно, здесь стоит отключить другие компоненты (Input, Movement, Combat)
        // чтобы предотвратить дальнейшие действия после смерти
        // _playerInput.enabled = false;
        // _playerMovement.enabled = false;
        // _playerCombat.enabled = false;
    }
}
