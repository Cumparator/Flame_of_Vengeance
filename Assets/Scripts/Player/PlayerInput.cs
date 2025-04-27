using UnityEngine;
using UnityEngine.InputSystem; // Не забудь эту строку для новой системы ввода!

// Добавляем RequireComponent, чтобы убедиться, что у игрока есть нужные компоненты в будущем
// [RequireComponent(typeof(PlayerMovement))]
// [RequireComponent(typeof(PlayerCombat))]
// [RequireComponent(typeof(PlayerAnimator))]
public class PlayerInput : MonoBehaviour // Убедись, что наследуется от MonoBehaviour
{
    // --- Публичные Свойства (для других скриптов) ---
    public Vector2 MoveInput { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right; // Начнем смотреть вправо
    public bool JumpTriggered { get; private set; }
    public bool RollTriggered { get; private set; }
    public bool BlockHeld { get; private set; }
    public bool MeleeTriggered { get; private set; }
    public bool RangedTriggered { get; private set; }

    // --- Приватные Поля ---
    private PlayerInputActions _inputActions;
    private Vector2 _rawLookInput; // Храним "сырой" ввод от стрелок/правого стика
    private Vector2 _lastNonZeroMoveInput = Vector2.right; // Запоминаем последнее направление движения
    private Vector2 _lastNonZeroRawLookInput = Vector2.right; // Запоминаем последнее направление взгляда со стрелок

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        // Создаем экземпляр нашего сгенерированного класса
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // --- Подписываемся на события ввода ---

        // Движение (Move)
        _inputActions.Gameplay.Move.performed += OnMovePerformed;
        _inputActions.Gameplay.Move.canceled += OnMoveCanceled;

        // Взгляд/Прицеливание (Look)
        _inputActions.Gameplay.Look.performed += OnLookPerformed;
        _inputActions.Gameplay.Look.canceled += OnLookCanceled;

        // Прыжок (Jump)
        _inputActions.Gameplay.Jump.performed += OnJumpPerformed;

        // Перекат (Roll)
        _inputActions.Gameplay.Roll.performed += OnRollPerformed;

        // Ближняя Атака (MeleeAttack)
        _inputActions.Gameplay.MeleeAttack.performed += OnMeleeAttackPerformed;

        // Дальняя Атака (RangedAttack)
        _inputActions.Gameplay.RangedAttack.performed += OnRangedAttackPerformed;

        // Блок (Block)
        _inputActions.Gameplay.Block.performed += OnBlockPerformed;
        _inputActions.Gameplay.Block.canceled += OnBlockCanceled;

        // Включаем карту действий "Gameplay"
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        // --- Отписываемся от всех событий ---
        _inputActions.Gameplay.Move.performed -= OnMovePerformed;
        _inputActions.Gameplay.Move.canceled -= OnMoveCanceled;
        _inputActions.Gameplay.Look.performed -= OnLookPerformed;
        _inputActions.Gameplay.Look.canceled -= OnLookCanceled;
        _inputActions.Gameplay.Jump.performed -= OnJumpPerformed;
        _inputActions.Gameplay.Roll.performed -= OnRollPerformed;
        _inputActions.Gameplay.MeleeAttack.performed -= OnMeleeAttackPerformed;
        _inputActions.Gameplay.RangedAttack.performed -= OnRangedAttackPerformed;
        _inputActions.Gameplay.Block.performed -= OnBlockPerformed;
        _inputActions.Gameplay.Block.canceled -= OnBlockCanceled;

        // Выключаем карту действий
        _inputActions.Gameplay.Disable();
    }

    private void Update()
    {
        // --- Обрабатываем логику направления взгляда ---
        UpdateAimDirection();
    }

    private void LateUpdate()
    {
        // --- Сбрасываем флаги триггеров ---
        // Это важно, чтобы действие срабатывало только один раз за нажатие
        JumpTriggered = false;
        RollTriggered = false;
        MeleeTriggered = false;
        RangedTriggered = false;
    }

    // --- Обработчики Событий Ввода ---

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        if (MoveInput.sqrMagnitude > 0.01f) // Запоминаем последнее направление
        {
            _lastNonZeroMoveInput = MoveInput.normalized;
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

     private void OnLookPerformed(InputAction.CallbackContext context)
    {
        _rawLookInput = context.ReadValue<Vector2>();
         if (_rawLookInput.sqrMagnitude > 0.01f) // Запоминаем последнее направление
        {
            _lastNonZeroRawLookInput = _rawLookInput.normalized;
        }
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        _rawLookInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpTriggered = true;
    }

     private void OnRollPerformed(InputAction.CallbackContext context)
    {
        RollTriggered = true;
    }

    private void OnMeleeAttackPerformed(InputAction.CallbackContext context)
    {
        MeleeTriggered = true;
    }

     private void OnRangedAttackPerformed(InputAction.CallbackContext context)
    {
        RangedTriggered = true;
    }

    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        BlockHeld = true;
    }

     private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        BlockHeld = false;
    }

    // --- Вспомогательные Методы ---

    private void UpdateAimDirection()
    {
        // Приоритет у ввода взгляда (стрелки)
        if (_rawLookInput.sqrMagnitude > 0.01f)
        {
            AimDirection = _rawLookInput.normalized;
        }
        // Если взгляд не используется, используем направление движения
        else if (MoveInput.sqrMagnitude > 0.01f)
        {
            AimDirection = MoveInput.normalized;
        }
        // Если ни взгляд, ни движение не активны, используем последнее запомненное направление
        // Приоритет отдаем последнему направлению взгляда, затем последнему направлению движения
        else
        {
             if (_lastNonZeroRawLookInput != Vector2.zero) // Проверяем, было ли вообще движение взгляда
             {
                  AimDirection = _lastNonZeroRawLookInput;
             }
             else if (_lastNonZeroMoveInput != Vector2.zero) // Проверяем, было ли вообще движение
             {
                   AimDirection = _lastNonZeroMoveInput;
             }
             // Если не было ни того, ни другого (маловероятно в начале), останется Vector2.right
        }
    }
}
