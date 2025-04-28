using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Скорость снаряда")][SerializeField] private float speed = 5f;
    private int _damage = 10;
    private Vector3 _direction;

    [Tooltip("Время жизни снаряда в секундах")][SerializeField] private float lifetime = 5f; 

    public void Initialize(Vector3 targetDirection, int projectileDamage)
    {
        _direction = targetDirection.normalized; // Нормализуем на всякий случай
        _damage = projectileDamage;
        
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (_direction != Vector3.zero)
        {
            transform.position += _direction * speed * Time.deltaTime;
        }
        else
        {   // Если направление не задано (ошибка инициализации?), уничтожаем
            Debug.LogWarning("Projectile direction not set!", this);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if(other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}