using UnityEngine;
using System.Collections; // Добавляем для корутин

// Гарантируем наличие необходимых компонентов
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))] // Теперь аниматор точно нужен
[RequireComponent(typeof(PlayerStamina))] // Добавляем зависимость от стамины
// Позже добавим PlayerMovement и PlayerAnimator, если нужно будет проверять их состояние
// [RequireComponent(typeof(PlayerMovement))]
// [RequireComponent(typeof(PlayerAnimator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Общие Настройки Атаки")]
    [SerializeField] private LayerMask enemyLayer; // Слой, на котором находятся враги
    [SerializeField] private Transform firePoint; // Точка для спавна снарядов (если есть)

    [Header("Настройки Ближнего Боя")]
    [SerializeField] private float meleeAttackDistance = 0.5f; // Смещение центра проверки от игрока
    [SerializeField] private float meleeAttackRadius = 0.6f;   // Радиус круга атаки
    [SerializeField] private int meleeDamage = 50;
    [SerializeField] private float meleeRate = 1f; // Атак в секунду (1 / время перезарядки) - теперь это скорее минимальный интервал
    [SerializeField] private float meleeComboResetTime = 1.0f; // Время для сброса комбо
    [SerializeField] private float meleeDamageDelay = 0.15f; // Задержка перед нанесением урона (сек)
    [SerializeField] private GameObject meleeEffectPrefab; // Префаб эффекта удара

    [Header("Настройки Дальнего Боя")]
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда
    [SerializeField] private int rangedDamage = 25;
    [SerializeField] private float rangedRate = 1f; // Выстрелов в секунду

    [Header("Настройки Блока")]
    [SerializeField] private bool canBlock = true; // Включен ли блок?
    // Стоимость блока теперь берется из PlayerStamina
    // [SerializeField] private float blockInitialStaminaCost = 5f; 
    public bool IsBlocking { get; private set; } // Состояние блока для других скриптов (например, аниматора)

    // --- Приватные Поля --- 
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement; // Может понадобиться для проверки CanAttack
    private PlayerAnimator _playerAnimator; // Теперь ссылка обязательна
    private PlayerStamina _playerStamina; // Добавляем ссылку на стамину

    private float _nextMeleeTime = 0f;
    private float _nextRangedTime = 0f;
    private int _currentAttack = 0; // Счетчик для комбо
    private float _timeSinceLastAttack = 0f;

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<PlayerAnimator>(); // Получаем аниматор
        _playerStamina = GetComponent<PlayerStamina>(); // Получаем стамину
        TryGetComponent<PlayerMovement>(out _playerMovement);
    }

    private void Update()
    {
        // Увеличиваем таймер с момента последней атаки для сброса комбо
        _timeSinceLastAttack += Time.deltaTime;

        HandleAttackInput();
        HandleBlocking();
    }

    // --- Обработка Ввода и Логика --- 

    private void HandleAttackInput()
    {
        // Можно ли атаковать сейчас? (не в перекате, не блокируем и т.д.)
        if (!CanAttack())
        {
            return; 
        }

        // Получаем направление из PlayerInput
        Vector2 attackDirection = _playerInput.AimDirection; 

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
        if (_playerInput.RangedTriggered && Time.time >= _nextRangedTime)
        {
             _currentAttack = 0; // Сбрасываем комбо ближнего боя
            PerformRangedAttack(attackDirection);
            _nextRangedTime = Time.time + 1f / rangedRate;
        }
    }

    private void HandleBlocking()
    { 
        if (!canBlock) 
        {
            IsBlocking = false;
            return;
        } 
        
        bool previousBlockingState = IsBlocking;
        bool blockInputHeld = _playerInput.BlockHeld;

        // Пытаемся НАЧАТЬ блок
        if (blockInputHeld && !IsBlocking && CanBlock()) // Если зажата кнопка, не блокировали и можем начать
        {
            // Пытаемся потратить начальную стоимость блока
            if (_playerStamina.TrySpendStamina(_playerStamina.blockInitialStaminaCost))
            {
                IsBlocking = true; // Начинаем блок только если хватило стамины
            }
            // else Debug.Log("Failed to Block - Not enough stamina!");
        }
        // Пытаемся ЗАКОНЧИТЬ блок
        else if (!blockInputHeld && IsBlocking) // Если кнопка отпущена и блокировали
        {
            IsBlocking = false;
        }

        // Сообщаем аниматору об изменении состояния блока
        if(IsBlocking != previousBlockingState)
        {
             _playerAnimator.SetBlocking(IsBlocking);
             if(IsBlocking) Debug.Log("Blocking Started!");
             else Debug.Log("Blocking Stopped!");
        }
    }

    // --- Выполнение Атак --- 

    private void PerformMeleeAttack(Vector2 direction, int attackNumber)
    {
        Debug.Log($"Melee Attack {attackNumber} Started! Direction: {direction}");

        // 1. Запускаем анимацию
        _playerAnimator.TriggerMeleeAttack(attackNumber);

        // 2. Рассчитываем точку проверки и эффекта (заранее)
        Vector2 hitOrigin = (Vector2)transform.position + direction * meleeAttackDistance;

        // 3. Создаем визуальный эффект (сразу)
        if (meleeEffectPrefab != null)
        {
            Instantiate(meleeEffectPrefab, hitOrigin, Quaternion.identity); 
        }

        // 4. Запускаем корутину для нанесения урона с задержкой
        StartCoroutine(ApplyMeleeDamageAfterDelay(hitOrigin, meleeAttackRadius, meleeDamage));
    }

    // Корутина для отложенного урона
    private IEnumerator ApplyMeleeDamageAfterDelay(Vector2 origin, float radius, int damage)
    {
        // Ждем указанную задержку
        yield return new WaitForSeconds(meleeDamageDelay);

        Debug.Log("Applying Melee Damage Now...");
        // Ищем врагов в зоне поражения
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(origin, radius, enemyLayer);

        // Наносим урон найденным врагам
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth health = enemyCollider.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage); 
                Debug.Log($"Delayed Hit enemy: {enemyCollider.name}");
            }
        }
    }

    private void PerformRangedAttack(Vector2 direction)
    {
        Debug.Log("Ranged Attack Triggered! Direction: " + direction);
        _playerAnimator.TriggerRangedAttack();

        // Создание снаряда
        if (projectilePrefab != null)
        {   
            // Определяем точку спавна
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
            
            // Создаем снаряд
            GameObject projectileGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            // Настраиваем скрипт снаряда (используем PlayerProjectile из твоего кода)
            PlayerProjectile projScript = projectileGO.GetComponent<PlayerProjectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                projScript.damage = rangedDamage; // Устанавливаем урон из этого скрипта
                 // Устанавливаем слой снаряда, если нужно
                 // SpriteRenderer projRenderer = projectileGO.GetComponent<SpriteRenderer>();
                 // if (projRenderer != null) projRenderer.sortingLayerName = "PlayerProjectile";
            }
            else
            {
                 Debug.LogWarning("Projectile prefab does not have PlayerProjectile script!", projectileGO);
            }
        }
         else
        {
            Debug.LogWarning("Projectile prefab is not assigned!", this);
        }
    }

    // --- Вспомогательные Методы Проверки Состояния ---

    private bool CanAttack()
    {
        // Нельзя атаковать во время переката или блока
        bool isRolling = _playerMovement != null && _playerMovement.IsRolling;
        return !isRolling && !IsBlocking; // Добавь другие условия если нужно (оглушение и т.д.)
    }

     private bool CanBlock()
    { 
        // Проверяем не только перекат, но и достаточность стамины для НАЧАЛА блока
        bool isRolling = _playerMovement != null && _playerMovement.IsRolling;
        return !isRolling && _playerStamina.HasEnoughStamina(_playerStamina.blockInitialStaminaCost);
    }

    // --- Визуализация для Редактора --- 

    private void OnDrawGizmosSelected()
    {
        if (_playerInput == null) return; // Нужно для избежания ошибок до запуска игры

        Vector2 direction = _playerInput.AimDirection.normalized;
        if (direction == Vector2.zero) direction = Vector2.right; // Направление по умолчанию для гизмо

        // Рисуем зону ближней атаки
        Vector2 center = (Vector2)transform.position + direction * meleeAttackDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, meleeAttackRadius);

        // Можно добавить гизмо для точки выстрела (firePoint)
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)direction * 0.5f); // Линия направления
        }
    }
}
