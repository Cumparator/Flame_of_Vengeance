using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackDistance = 1.5f;
    public float attackRadius = 1.0f;
    public int meleeDamage = 50;
    public int rangedDamage = 25;
    public float meleeRate = 1f;
    public float rangedRate = 1f;
    public LayerMask enemyLayer;

    public GameObject meleeEffectPrefab;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Animator animator;
    private float nextMeleeTime = 0f;
    private float nextRangedTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 attackDir = GetInputDirection();

        if (attackDir != Vector2.zero)
        {
            if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextMeleeTime)
            {
                MeleeAttack(attackDir.normalized);
                nextMeleeTime = Time.time + meleeRate;
            }
            else if (Input.GetKeyDown(KeyCode.X) && Time.time >= nextRangedTime)
            {
                RangedAttack(attackDir.normalized);
                nextRangedTime = Time.time + rangedRate;
            }
        }
    }

    Vector2 GetInputDirection()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) v += 1;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1;
        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1;

        return new Vector2(h, v);
    }

    void MeleeAttack(Vector2 direction)
    {
        Vector2 origin = (Vector2)transform.position + direction * attackDistance;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(origin, attackRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(meleeDamage);
            }
        }

        if (animator != null)
        {
            animator.SetFloat("AttackX", direction.x);
            animator.SetFloat("AttackY", direction.y);
            animator.SetTrigger("Attack");
        }

        if (meleeEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + (Vector3)(direction * attackDistance);
            Instantiate(meleeEffectPrefab, effectPos, Quaternion.identity);
        }

        Debug.Log("Melee attack in direction: " + direction);
    }

    void RangedAttack(Vector2 direction)
    {
        if (animator != null)
        {
            animator.SetFloat("AttackX", direction.x);
            animator.SetFloat("AttackY", direction.y);
            animator.SetTrigger("Attack");
        }

        if (projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            PlayerProjectile projScript = projectile.GetComponent<PlayerProjectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                projScript.damage = rangedDamage;
            }
        }

        Debug.Log("Ranged attack in direction: " + direction);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 dir = GetInputDirection().normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        Vector2 center = (Vector2)transform.position + dir * attackDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}
