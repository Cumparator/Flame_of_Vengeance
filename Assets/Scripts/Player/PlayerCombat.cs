using System.Collections;
using FlameOfVengeance.Interfaces;
using UnityEngine;
// Добавляем для корутин

namespace Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Общие Настройки Атаки")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform firePoint;

        [Header("Настройки Ближнего Боя")]
        [SerializeField] private float meleeAttackDistance = 0.5f;
        [SerializeField] private float meleeAttackRadius = 0.6f;
        [SerializeField] private int meleeDamage = 50;
        [SerializeField] private float meleeRate = 1f;
        [SerializeField] private float meleeComboResetTime = 1.0f;
        [SerializeField] private float meleeDamageDelay = 0.15f;
        [SerializeField] private GameObject meleeEffectPrefab;

        [Header("Настройки Дальнего Боя")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int rangedDamage = 25;
        [SerializeField] private float rangedRate = 1f;

        [Header("Настройки Блока")]
        [SerializeField] private bool canBlock = true;
        public bool IsBlocking { get; private set; }

        private PlayerInput _playerInput;
        private PlayerMovement _playerMovement;
        private PlayerAnimator _playerAnimator; 
        private PlayerStamina _playerStamina;

        private float _nextMeleeTime = 0f;
        private float _nextRangedTime = 0f;
        private int _currentAttack = 0;
        private float _timeSinceLastAttack = 0f;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            _playerStamina = GetComponent<PlayerStamina>();
            TryGetComponent(out _playerMovement);
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;

            HandleAttackInput();
            HandleBlocking();
        }

        private void HandleAttackInput()
        {
            if (!CanAttack())
            {
                return; 
            }

            var attackDirection = _playerInput.AimDirection; 

            // Ближняя атака (Комбо)
            if (_playerInput.MeleeTriggered && Time.time >= _nextMeleeTime)
            {
                // Сбрасываем комбо, если прошло много времени
                if (_timeSinceLastAttack > meleeComboResetTime)
                {
                    _currentAttack = 0;
                }

                _currentAttack++;

                // Возвращаем на первую атаку после третьей
                if (_currentAttack > 3)
                {
                    _currentAttack = 1;
                }

                // Выполняем атаку
                PerformMeleeAttack(attackDirection, _currentAttack);

                // Сбрасываем таймеры
                _timeSinceLastAttack = 0f;
                _nextMeleeTime = Time.time + 1f / meleeRate; 
            }

            // Дальняя атака (прерывает комбо ближнего боя)
            if (!_playerInput.RangedTriggered || !(Time.time >= _nextRangedTime)) return;
            _currentAttack = 0; // Сбрасываем комбо ближнего боя
            PerformRangedAttack(attackDirection);
            _nextRangedTime = Time.time + 1f / rangedRate;
        }

        private void HandleBlocking()
        { 
            if (!canBlock) 
            {
                IsBlocking = false;
                return;
            } 
        
            var previousBlockingState = IsBlocking;
            var blockInputHeld = _playerInput.BlockHeld;

            switch (blockInputHeld)
            {
                case true when !IsBlocking && CanBlock():
                {
                    if (_playerStamina.TrySpendStamina(_playerStamina.blockInitialStaminaCost))
                    {
                        IsBlocking = true;
                    }

                    break;
                }
                case false when IsBlocking:
                    IsBlocking = false;
                    break;
            }

            if(IsBlocking != previousBlockingState)
            {
                _playerAnimator.SetBlocking(IsBlocking);
            }
        }

        private void PerformMeleeAttack(Vector2 direction, int attackNumber)
        {
            _playerAnimator.TriggerMeleeAttack(attackNumber);

            var hitOrigin = (Vector2)transform.position + direction * meleeAttackDistance;

            if (meleeEffectPrefab)
            {
                Instantiate(meleeEffectPrefab, hitOrigin, Quaternion.identity); 
            }

            StartCoroutine(ApplyMeleeDamageAfterDelay(hitOrigin, meleeAttackRadius, meleeDamage));
        }

        // Корутина для отложенного урона
        private IEnumerator ApplyMeleeDamageAfterDelay(Vector2 origin, float radius, int damage)
        {
            yield return new WaitForSeconds(meleeDamageDelay);

            var hitEnemies = Physics2D.OverlapCircleAll(origin, radius, enemyLayer);

            foreach (var enemyCollider in hitEnemies)
            {
                var damageableObject = enemyCollider.GetComponent<IDamageable>();
                damageableObject?.TakeDamage(damage);
            }
        }

        private void PerformRangedAttack(Vector2 direction)
        {
            _playerAnimator.TriggerRangedAttack();

            if (!projectilePrefab) return;
            var spawnPos = firePoint ? firePoint.position : transform.position;
            
            var projectileGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            var projScript = projectileGO.GetComponent<PlayerProjectile>();
            if (!projScript) return;
            projScript.SetDirection(direction);
            projScript.damage = rangedDamage;
        }

        private bool CanAttack()
        {
            var isRolling = _playerMovement && _playerMovement.IsRolling;
            return !isRolling && !IsBlocking;
        }

        private bool CanBlock()
        { 
            var isRolling = _playerMovement && _playerMovement.IsRolling;
            return !isRolling && _playerStamina.HasEnoughStamina(_playerStamina.blockInitialStaminaCost);
        }

        private void OnDrawGizmosSelected()
        {
            if (_playerInput == null) return;

            var direction = _playerInput.AimDirection.normalized;
            if (direction == Vector2.zero) direction = Vector2.right;

            var center = (Vector2)transform.position + direction * meleeAttackDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, meleeAttackRadius);

            if (firePoint == null) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)direction * 0.5f);
        }
    }
}