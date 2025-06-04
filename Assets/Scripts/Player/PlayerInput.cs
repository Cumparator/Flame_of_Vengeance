using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(PlayerMovement))]
// [RequireComponent(typeof(PlayerCombat))]
// [RequireComponent(typeof(PlayerAnimator))]
public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public bool JumpTriggered { get; private set; }
    public bool RollTriggered { get; private set; }
    public bool BlockHeld { get; private set; }
    public bool MeleeTriggered { get; private set; }
    public bool RangedTriggered { get; private set; }

    private PlayerInputActions _inputActions;
    private Vector2 _rawLookInput;
    private Vector2 _lastNonZeroMoveInput = Vector2.right;
    private Vector2 _lastNonZeroRawLookInput = Vector2.right;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Move.performed += OnMovePerformed;
        _inputActions.Gameplay.Move.canceled += OnMoveCanceled;

        _inputActions.Gameplay.Look.performed += OnLookPerformed;
        _inputActions.Gameplay.Look.canceled += OnLookCanceled;

        _inputActions.Gameplay.Jump.performed += OnJumpPerformed;

        _inputActions.Gameplay.Roll.performed += OnRollPerformed;

        _inputActions.Gameplay.MeleeAttack.performed += OnMeleeAttackPerformed;

        _inputActions.Gameplay.RangedAttack.performed += OnRangedAttackPerformed;

        _inputActions.Gameplay.Block.performed += OnBlockPerformed;
        _inputActions.Gameplay.Block.canceled += OnBlockCanceled;

        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
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

        _inputActions.Gameplay.Disable();
    }

    private void Update()
    {
        UpdateAimDirection();
    }

    private void LateUpdate()
    {
        JumpTriggered = false;
        RollTriggered = false;
        MeleeTriggered = false;
        RangedTriggered = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        if (MoveInput.sqrMagnitude > 0.01f)
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
         if (_rawLookInput.sqrMagnitude > 0.01f)
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

    private void UpdateAimDirection()
    {
        if (_rawLookInput.sqrMagnitude > 0.01f)
        {
            AimDirection = _rawLookInput.normalized;
        }
        else if (MoveInput.sqrMagnitude > 0.01f)
        {
            AimDirection = MoveInput.normalized;
        }
    }
}