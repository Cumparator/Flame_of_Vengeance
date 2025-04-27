using UnityEngine;

// Гарантируем наличие необходимых компонентов на игровом объекте
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerStamina))] // Добавляем зависимость от стамины
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки Движения")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Настройки Прыжка/Уворота")]
    [SerializeField] private float jumpForce = 8f; // Сила для 'прыжка' (может быть визуальным или коротким рывком)
    // Добавь [SerializeField] private float jumpDuration = 0.2f; если прыжок имеет длительность

    [Header("Настройки Переката/Рывка")]
    [SerializeField] private float rollSpeed = 10f; // Скорость во время переката
    [SerializeField] private float rollDuration = 0.5f; // Длительность переката в секундах

    // --- Публичные Свойства (для других скриптов, особенно Аниматора) ---
    public bool IsRolling { get; private set; }
    public Vector2 CurrentVelocity => _rb.linearVelocity;
    public Vector2 FacingDirection { get; private set; } = Vector2.right; // Направление, куда смотрит

    // --- Приватные Поля ---
    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private PlayerAnimator _playerAnimator; // Теперь ссылка используется
    private PlayerStamina _playerStamina; // Добавляем ссылку на стамину

    private bool _isJumping = false; // Флаг для состояния прыжка (если он длительный)
    private float _rollTimer = 0f;
    private Vector2 _rollDirection = Vector2.zero;

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        // Получаем ссылки на компоненты
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<PlayerAnimator>(); // Получаем аниматор
        _playerStamina = GetComponent<PlayerStamina>(); // Получаем стамину
    }

    private void Update()
    {
        // Обновляем направление взгляда (может понадобиться для анимаций)
        FacingDirection = _playerInput.AimDirection;

        // Обработка начала переката (реагируем на триггер из PlayerInput)
        HandleRollStart();

        // Обработка прыжка (реагируем на триггер из PlayerInput)
        HandleJumpStart();

        // Обновление таймера и состояния переката
        UpdateRollState();
    }

    private void FixedUpdate()
    {
        // Физические расчеты лучше делать в FixedUpdate
        HandleMovement();
        HandleRollMovement();
        // Возможно, HandleJumpMovement() если прыжок влияет на физику
    }

    // --- Основные Обработчики ---

    private void HandleMovement()
    {
        // Двигаемся только если не в перекате и не в прыжке (если прыжок блокирует движение)
        if (!IsRolling && !_isJumping) // Добавь другие состояния, блокирующие движение, если нужно
        {
            Vector2 targetVelocity = _playerInput.MoveInput * moveSpeed;
            // Используем velocity для более динамичного ощущения.
            // Можно добавить небольшое сглаживание, если нужно.
             _rb.linearVelocity = new Vector2(targetVelocity.x, targetVelocity.y);
            // Если нужна физика с гравитацией (маловероятно для top-down), сохраняем Y:
            // _rb.velocity = new Vector2(targetVelocity.x, _rb.velocity.y);
        }
        else if(!IsRolling) // Если в прыжке, но не в перекате, обнуляем скорость (или специфичная логика прыжка)
        {
             _rb.linearVelocity = Vector2.zero; // Пример: остановка во время "прыжка"
        }
    }

     private void HandleJumpStart()
     {
        // Начинаем прыжок, если получена команда и персонаж может прыгать (не в перекате и т.д.)
        if (_playerInput.JumpTriggered && CanJump())
        {
            _isJumping = true; // Устанавливаем флаг
            _playerAnimator.TriggerJump(); // Вызываем метод аниматора
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Пример: небольшой вертикальный толчок
            Debug.Log("Jump Triggered!");
            // 3. Временная неуязвимость? (логика в PlayerCombat)
            // 4. Сброс флага _isJumping через время или по анимации/приземлению (если нужно)
             Invoke(nameof(StopJumping), 0.1f); // Пример: очень короткий прыжок
        }
     }
     private void StopJumping() // Метод для завершения состояния прыжка
     {
        _isJumping = false;
     }

    private void HandleRollStart()
    {
        // Начинаем перекат, если есть команда И достаточно стамины
        if (_playerInput.RollTriggered && CanRoll())
        {
            // Пытаемся потратить стамину
            if (_playerStamina.TrySpendStamina(_playerStamina.rollStaminaCost))
            {
                // Стамины хватило, начинаем перекат
                IsRolling = true;
                _rollTimer = rollDuration;

                // Определяем направление переката: приоритет у ввода движения, затем у взгляда
                if (_playerInput.MoveInput.sqrMagnitude > 0.1f)
                {
                    _rollDirection = _playerInput.MoveInput.normalized;
                }
                else
                {
                    _rollDirection = _playerInput.AimDirection.normalized; // Используем направление взгляда если стоим
                }

                _playerAnimator.TriggerRoll(); // Вызываем метод аниматора
                Debug.Log("Roll Triggered! Direction: " + _rollDirection);
            }
            // else Debug.Log("Failed to Roll - Not enough stamina!");
        }
    }

    private void UpdateRollState()
    {
        // Уменьшаем таймер переката, если он активен
        if (IsRolling)
        {
            _rollTimer -= Time.deltaTime;
            if (_rollTimer <= 0f)
            {
                IsRolling = false;
                // Возможно, запуск анимации завершения переката
                // _playerAnimator.FinishRoll();
            }
        }
    }

     private void HandleRollMovement()
    {
        // Применяем скорость переката, если персонаж катится
        if (IsRolling)
        {
            _rb.linearVelocity = _rollDirection * rollSpeed;
        }
    }


    // --- Вспомогательные Методы Проверки Состояния ---

    private bool CanMove()
    {
        // Можно двигаться, если не в перекате (и не в других блокирующих состояниях)
        return !IsRolling && !_isJumping;
    }

    private bool CanRoll()
    {
        // Добавляем проверку стамины
        // (Проверка HasEnoughStamina здесь не обязательна, т.к. есть TrySpendStamina,
        // но может быть полезна для UI, чтобы показать, возможно ли действие)
        return !IsRolling && !_isJumping && _playerStamina.HasEnoughStamina(_playerStamina.rollStaminaCost);
    }

     private bool CanJump()
    {
        // Можно прыгать, если не в перекате и не в прыжке (или другие условия?)
         return !IsRolling && !_isJumping; // Добавь && isGrounded если есть понятие земли
    }
}
