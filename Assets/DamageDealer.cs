using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageAmount = 10;

    void OnCollisionEnter2D(Collision2D collision)
    {
        var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }
        print("Collision detected");
    }
}