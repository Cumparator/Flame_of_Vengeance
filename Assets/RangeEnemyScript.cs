using UnityEngine;


public class RangeEnemy : MonoBehaviour
{
    [Header("Бой")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int projectileDamage = 10;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float detectionRadius = 9.0f;

    private Transform _player;
    private float _nextShotTime;
    private bool _canSeePlayer;
    private float _distanceToPlayer;
    private Vector2 _directionToPlayer;

    private BanditAI _banditAI;

    void Start()
    {
        TryGetComponent(out _banditAI);

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
        if (!_player || (_banditAI != null && _banditAI.IsDead)) return;

        UpdateTargetInfo();
        HandleShooting();
    }

    private void UpdateTargetInfo()
    {
        _distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        _directionToPlayer = (_player.position - transform.position).normalized;

        var hit = Physics2D.Raycast(transform.position, _directionToPlayer, _distanceToPlayer, wallMask);
        _canSeePlayer = !hit.collider;
    }

    private void HandleShooting()
    {
        if ((_banditAI != null && _banditAI.IsDead) || !_canSeePlayer || _distanceToPlayer > detectionRadius || !(Time.time >= _nextShotTime)) return;
        Shoot();
        _nextShotTime = Time.time + shootInterval;
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
