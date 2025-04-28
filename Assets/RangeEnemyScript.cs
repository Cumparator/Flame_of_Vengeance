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
    [SerializeField] private int projectileDamage = 10; // Добавляем урон снаряда для врага
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
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
        else
        {
            enabled = false;
            return;
        }
        _nextShotTime = Time.time + shootInterval;
    }

    void Update()
    {
        if (!_player) return;

        UpdateTargetInfo();
        HandleMovement();
        HandleShooting();
    }

    private void UpdateTargetInfo()
    {
        _distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        _directionToPlayer = (_player.position - transform.position).normalized;

        // Единая проверка raycast на видимость и препятствия
        var hit = Physics2D.Raycast(transform.position, _directionToPlayer, _distanceToPlayer, wallMask);
        _canSeePlayer = !hit.collider;
    }

    private void HandleMovement()
    {
        if (!_canSeePlayer) return;
        
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

    private void HandleShooting()
    {
        if (!_canSeePlayer || !(Time.time >= _nextShotTime)) return;
        Shoot();
        _nextShotTime = Time.time + shootInterval;
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

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;
        var projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, _directionToPlayer));

        var projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript)
        {
            projectileScript.Initialize(_directionToPlayer, projectileDamage);
        }
    }
}
