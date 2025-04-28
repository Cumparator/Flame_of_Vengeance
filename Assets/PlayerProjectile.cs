using UnityEngine;
using FlameOfVengeance.Interfaces;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 25;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, 1f); // ��������������� ����� 2 �������
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
        // Ищем любой компонент, реализующий IDamageable
        IDamageable damageableObject = other.GetComponent<IDamageable>(); 
        if (damageableObject != null)
        {
            // Если нашли такой объект, наносим ему урон
            damageableObject.TakeDamage(damage); 
            Destroy(gameObject); // Уничтожаем снаряд
        }
        // Можно добавить проверку на другие типы объектов, если нужно
        // else if (other.CompareTag("Obstacle")) { Destroy(gameObject); }
    }
}
