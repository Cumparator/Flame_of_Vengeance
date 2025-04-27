using UnityEngine;

// Гарантируем наличие необходимых компонентов
[RequireComponent(typeof(PlayerInput))]
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
    [SerializeField] private float meleeRate = 1f; // Атак в секунду (1 / время перезарядки)
    [SerializeField] private GameObject meleeEffectPrefab; // Префаб эффекта удара

    [Header("Настройки Дальнего Боя")]
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда
    [SerializeField] private int rangedDamage = 25;
    [SerializeField] private float rangedRate = 1f; // Выстрелов в секунду

    [Header("Настройки Блока")]
    [SerializeField] private bool canBlock = true; // Включен ли блок?
    // Добавь сюда параметры блока, если нужно (например, снижение урона)
    public bool IsBlocking { get; private set; } // Состояние блока для других скриптов (например, аниматора)

    // --- Приватные Поля --- 
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement; // Может понадобиться для проверки CanAttack
    // private PlayerAnimator _playerAnimator; // Для запуска анимаций атаки/блока

    private float _nextMeleeTime = 0f;
    private float _nextRangedTime = 0f;

    // --- Методы Жизненного Цикла Unity ---

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        // Попробуем получить PlayerMovement, если он есть
        TryGetComponent<PlayerMovement>(out _playerMovement);
        // _playerAnimator = GetComponent<PlayerAnimator>(); // Получим позже
    }

    private void Update()
    {
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

        // Ближняя атака
        if (_playerInput.MeleeTriggered && Time.time >= _nextMeleeTime)
        {
            PerformMeleeAttack(attackDirection);
            _nextMeleeTime = Time.time + 1f / meleeRate; // Рассчитываем время следующей атаки
        }

        // Дальняя атака
        if (_playerInput.RangedTriggered && Time.time >= _nextRangedTime)
        {
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
        
        // Можно ли блокировать? (не в перекате и т.д.)
        if (CanBlock())
        {
            IsBlocking = _playerInput.BlockHeld;
            // Возможно, здесь нужно сообщить аниматору
            // _playerAnimator.SetBlocking(IsBlocking);
            if(IsBlocking) Debug.Log("Blocking!");
        }
        else
        {
            IsBlocking = false;
             // _playerAnimator.SetBlocking(false);
        }
    }

    // --- Выполнение Атак --- 

    private void PerformMeleeAttack(Vector2 direction)
    {
        Debug.Log("Melee Attack Triggered! Direction: " + direction);

        // Рассчитываем точку проверки попадания
        Vector2 origin = (Vector2)transform.position + direction * meleeAttackDistance;
        
        // Ищем врагов в зоне поражения
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(origin, meleeAttackRadius, enemyLayer);

        // Наносим урон найденным врагам
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // Пытаемся получить компонент здоровья врага
            // Рассмотри использование интерфейса IDamageable для гибкости
            EnemyHealth health = enemyCollider.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(meleeDamage); 
                Debug.Log($"Hit enemy: {enemyCollider.name}");
            }
        }

        // Запуск анимации (позже, через PlayerAnimator)
        // _playerAnimator.TriggerMeleeAttack(direction);

        // Создание визуального эффекта
        if (meleeEffectPrefab != null)
        {
             // Эффект в точке контакта или на персонаже?
            Vector3 effectPos = origin; // Пример: эффект в центре зоны поражения
            // Возможно, повернуть эффект по направлению атаки?
            // Quaternion effectRotation = Quaternion.LookRotation(Vector3.forward, direction); 
            Instantiate(meleeEffectPrefab, effectPos, Quaternion.identity); 
        }
    }

    private void PerformRangedAttack(Vector2 direction)
    {
        Debug.Log("Ranged Attack Triggered! Direction: " + direction);

        // Запуск анимации (позже)
        // _playerAnimator.TriggerRangedAttack(direction);

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
        // Нельзя блокировать во время переката
        bool isRolling = _playerMovement != null && _playerMovement.IsRolling;
        return !isRolling; // Добавь другие условия если нужно
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
