using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Скорость снаряда")][SerializeField] private float speed = 5f;
    private int _damage = 10;
    private Vector3 _direction;

    [Tooltip("Время жизни снаряда в секундах")][SerializeField] private float lifetime = 5f;
    
    [Tooltip("Префаб эффекта при столкновении (например, взрыв)")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Tooltip("Смещение позиции эффекта при попадании относительно центра снаряда. Используется для точной подстройки.")]
    [SerializeField] private float hitEffectOffset = 0.2f;

    public void Initialize(Vector3 targetDirection, int projectileDamage)
    {
        _direction = targetDirection.normalized; // Нормализуем на всякий случай
        _damage = projectileDamage;
        
        // Поворачиваем снаряд в сторону движения (предполагая, что спрайт изначально "смотрит" вправо)
        transform.rotation = Quaternion.FromToRotation(Vector3.left, _direction);
        
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
            Debug.LogWarning("Projectile direction not set! Destroying.", this);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Projectile hit: {other.name} with tag {other.tag}", other.gameObject);
        if (other.CompareTag("Wall"))
        {
            InstantiateHitEffect(); // Создаем эффект перед уничтожением
            Destroy(gameObject);
            return;
        }

        if(other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(_damage);
            InstantiateHitEffect(); // Создаем эффект перед уничтожением
            Destroy(gameObject);
        }
    }

    // Метод для создания эффекта попадания
    private void InstantiateHitEffect()
    {
        Debug.Log($"Attempting to instantiate hit effect. Prefab assigned: {hitEffectPrefab != null}", this);
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position - _direction * hitEffectOffset;

            Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }
}