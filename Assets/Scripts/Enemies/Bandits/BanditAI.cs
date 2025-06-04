using UnityEngine;
using System.Collections;
using FlameOfVengeance.Interfaces;

public class BanditAI : MonoBehaviour {
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Recover1 = Animator.StringToHash("Recover");

    [Header("Параметры AI")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float detectionRadius = 8.0f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float attackRate = 1.0f;

    [Header("Настройки Атаки")]
    [SerializeField] private LayerMask playerLayer; // Слой игрока
    [SerializeField] private int attackDamage = 20; // Урон, наносимый бандитом
    [SerializeField] private float banditDamageDelay = 0.3f; // Задержка перед нанесением урона
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f); // Смещение точки проверки атаки

    private Animator _animator;
    private Rigidbody2D _rb;
    private Transform _player;
    private bool _isPlayerInRange = false;
    private bool _isPlayerInAttackRange = false;
    private bool _isFacingRight = false;
    private float _nextAttackTime = 0f;
    private bool _isDead = false;

    public bool IsDead => _isDead;

    void Start () {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        FindPlayer();
    }
    
	void Update () {
        if (_isDead || !_player)
        {
             if(_isDead) _rb.linearVelocity = Vector2.zero;
            return;
        }

        var distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        _isPlayerInRange = distanceToPlayer <= detectionRadius;
        _isPlayerInAttackRange = distanceToPlayer <= attackRadius;

        FacePlayer();

        if (_isPlayerInAttackRange)
        {
            AttackState();
        }
        else if (_isPlayerInRange)
        {
            ChaseState();
        }
        else
        {
            IdleState();
        }
    }

    void IdleState()
    {
        _rb.linearVelocity = Vector2.zero;
        _animator.SetInteger(AnimState, 0);
    }

    void ChaseState()
    {
        Vector2 directionToPlayer = (_player.position - transform.position).normalized;
        _rb.linearVelocity = directionToPlayer * moveSpeed;
        
        // Анимация Combat Idle (состояние 1) или Run (состояние 2)?
        // Пока используем CombatIdle как состояние "настороже"
        _animator.SetInteger(AnimState, 2);
    }

    void AttackState()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        _animator.SetInteger(AnimState, 1);

        if (!(Time.time >= _nextAttackTime)) return;
        _animator.SetTrigger(Attack);
        StartCoroutine(ApplyBanditDamageAfterDelay(attackRadius, attackDamage)); // Запускаем корутину для нанесения урона с задержкой
        _nextAttackTime = Time.time + 1f / attackRate;
    }

    // Корутина для отложенного нанесения урона игроку
    private IEnumerator ApplyBanditDamageAfterDelay(float radius, int damage)
    {
        yield return new WaitForSeconds(banditDamageDelay);

        Vector2 currentAttackOffset = _isFacingRight ? attackOffset : new Vector2(-attackOffset.x, attackOffset.y);
        Vector2 attackOrigin = (Vector2)transform.position + currentAttackOffset;

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackOrigin, radius, playerLayer);

        if (hitPlayer)
        {
            IDamageable playerDamageable = hitPlayer.GetComponent<IDamageable>();
            playerDamageable?.TakeDamage(damage);
        }
    }

    void FindPlayer()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
        {
            _player = playerObject.transform;
        }
    }

    void FacePlayer()
    {
        var playerIsRight = _player.position.x > transform.position.x;

        switch (playerIsRight)
        {
            case true when !_isFacingRight:
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                _isFacingRight = true;
                break;
            
            case false when _isFacingRight:
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                _isFacingRight = false;
                break;
        }
    }

    public void Die()
    {
        if (_isDead) return;

        _isDead = true;
        _animator.SetTrigger(Death);
        GetComponent<Collider2D>().enabled = false;
        _rb.simulated = false;
    }

    public void Recover() 
    {
        if (!_isDead) return;
        _isDead = false;
        _animator.SetTrigger(Recover1);
        GetComponent<Collider2D>().enabled = true;
        _rb.simulated = true;
    }
}