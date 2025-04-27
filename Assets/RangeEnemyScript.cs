using UnityEngine;

// Рассмотрите использование RequireComponent для Rigidbody2D, если планируется движение на основе физики
// [RequireComponent(typeof(Rigidbody2D))]
public class RangeEnemy : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float distanceBuffer = 1f; // Буфер для предотвращения дрожания

    [Header("Бой")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask wallMask; // Слой маски для препятствий

    // Рассмотрите внедрение зависимости от игрока вместо поиска по тегу для лучшей тестируемости
    private Transform _player;
    private float _nextShotTime;
    private bool _canSeePlayer;
    private float _distanceToPlayer;
    private Vector2 _directionToPlayer;

    void Start()
    {
        // Ищем объект игрока - рассмотрите внедрение зависимостей для лучшего разделения
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Игрок не найден! Врагу RangeEnemy нужна цель.", this);
            // Отключаем врага, если игрок не найден, чтобы предотвратить ошибки
            enabled = false;
            return;
        }
        _nextShotTime = Time.time + shootInterval; // Инициализируем время следующего выстрела
    }

    void Update()
    {
        if (_player == null) return; // Не должно произойти, если логика в Start верна, но безопасность прежде всего

        UpdateTargetInfo();
        HandleMovement();
        HandleShooting();
    }

    private void UpdateTargetInfo()
    {
        _distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        _directionToPlayer = (_player.position - transform.position).normalized;

        // Единая проверка raycast на видимость и препятствия
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _directionToPlayer, _distanceToPlayer, wallMask);
        _canSeePlayer = hit.collider == null; // Игрок виден, если луч не столкнулся со стеной
    }

    private void HandleMovement()
    {
        // Двигаться только если игрок виден (или решить, как вести себя, когда игрок скрыт)
        if (_canSeePlayer)
        {
            if (_distanceToPlayer > preferredDistance + distanceBuffer)
            {
                MoveTowards(_directionToPlayer);
            }
            else if (_distanceToPlayer < preferredDistance - distanceBuffer)
            {
                MoveAway(_directionToPlayer);
            }
            // Опционально: Добавить стрейф или другую логику движения здесь
        }
        // else
        // {
        //     // Опционально: Определить поведение, когда игрок не виден (например, патрулирование, поиск)
        // }
    }

    private void HandleShooting()
    {
        // Стрелять, только если игрок виден и прошел интервал
        if (_canSeePlayer && Time.time >= _nextShotTime)
        {
            Shoot();
            _nextShotTime = Time.time + shootInterval; // Сбрасываем таймер для следующего выстрела
        }
    }

    void MoveTowards(Vector2 direction)
    {
        // Используем transform.position для простоты; рассмотрите Rigidbody2D.MovePosition для взаимодействия с физикой
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    void MoveAway(Vector2 direction)
    {
        // Используем transform.position для простоты; рассмотрите Rigidbody2D.MovePosition для взаимодействия с физикой
        transform.position -= (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    // Логика CanSeePlayer теперь интегрирована в UpdateTargetInfo

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Рассмотрите использование пула объектов для снарядов для повышения производительности
            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, _directionToPlayer)); // Поворачиваем снаряд в сторону игрока

            // Получаем SpriteRenderer созданного снаряда
            SpriteRenderer projectileRenderer = projectileGO.GetComponent<SpriteRenderer>();
            if (projectileRenderer != null)
            {
                // Устанавливаем сортировочный слой
                projectileRenderer.sortingLayerName = "Bullet"; // Имя слоя с большой буквы
            }
            else
            {
                // Если у префаба снаряда нет SpriteRenderer, сообщим об этом
                Debug.LogWarning("У префаба снаряда отсутствует компонент SpriteRenderer.", projectileGO);
            }

            // Возможно, добавить эффект вспышки или звук выстрела здесь
        }
        else
        {
            if(projectilePrefab == null) Debug.LogWarning("Префаб снаряда не назначен.", this);
            if(firePoint == null) Debug.LogWarning("Точка выстрела не назначена.", this);
        }
    }
}
