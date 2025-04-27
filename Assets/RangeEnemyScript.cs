using UnityEngine;

public class RangeEnemy : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float preferredDistance = 5f;
    public float shootInterval = 2f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public LayerMask wallMask; // <- Добавили маску для стен

    private Transform _player;
    private float _lastShotTime;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _lastShotTime = Time.time;
    }

    void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Движение с обходом препятствий
        Vector2 direction = (_player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceToPlayer, wallMask);

        if (hit.collider == null) // Ничего не мешает — можно двигаться
        {
            if (distanceToPlayer > preferredDistance + 1f)
                MoveTowards(direction);
            else if (distanceToPlayer < preferredDistance - 1f)
                MoveAway(direction);
        }

        // Атака, если виден игрок
        if (Time.time - _lastShotTime >= shootInterval)
        {
            if (CanSeePlayer())
            {
                Shoot();
                _lastShotTime = Time.time;
            }
        }
    }

    void MoveTowards(Vector2 direction)
    {
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    void MoveAway(Vector2 direction)
    {
        transform.position -= (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    bool CanSeePlayer()
    {
        Vector2 start = transform.position;
        Vector2 end = _player.position;
        RaycastHit2D hit = Physics2D.Linecast(start, end, wallMask);
        return hit.collider == null; // Если ничего не мешает — игрок в поле зрения
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        }
    }
}
