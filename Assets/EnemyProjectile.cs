using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    private Vector3 direction;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            direction = (player.transform.position - transform.position).normalized;

        Destroy(gameObject, 5f); // Самоуничтожение через 5 секунд
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall")) // Если попала в стену
        {
            Destroy(gameObject);
            return;
        }

        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
