using UnityEngine;
using FlameOfVengeance.Interfaces;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] public float speed = 10f;
    [SerializeField] public int damage = 25;
    private Vector2 direction;

    [Tooltip("Префаб эффекта при столкновении (например, взрыв)")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Tooltip("Смещение позиции эффекта при попадании относительно центра снаряда. Используется для точной подстройки.")]
    [SerializeField] private float hitEffectOffset = 0.2f;

    void Start()
    {
        Destroy(gameObject, 1f);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ищем любой компонент, реализующий IDamageable
        IDamageable damageableObject = other.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            // Если нашли такой объект, наносим ему урон
            damageableObject.TakeDamage(damage);
            InstantiateHitEffect();
            Destroy(gameObject); // Уничтожаем снаряд
        }
        else if (other.CompareTag("Wall"))
        {
            InstantiateHitEffect();
            Destroy(gameObject);
        }
    }

    private void InstantiateHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position - (Vector3)direction * hitEffectOffset;

            Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
