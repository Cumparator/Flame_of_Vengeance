using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 25;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, 1f); // автоуничтожение через 2 секунды
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // уничтожаем пулю при попадании
        }
    }
}
