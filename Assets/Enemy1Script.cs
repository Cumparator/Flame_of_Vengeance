using UnityEngine;

public class Enemy1Script : MonoBehaviour
{
    public float Velocity = 10.0f;
    public LayerMask wallMask;

    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Vector3 direction = (_player.position - transform.position).normalized;

        // ѕровер€ем, не мешает ли что-то на пути
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, wallMask);

        if (hit.collider == null)
        {
            // Ќет стены Ч идЄм напр€мую
            transform.position += direction * (Velocity * Time.deltaTime);
        }
        else
        {
            // —тена мешает Ч пробуем обойти (влево/вправо)
            Vector3 altDir1 = new Vector3(-direction.y, direction.x); // ѕерпендикул€р слева
            Vector3 altDir2 = new Vector3(direction.y, -direction.x); // ѕерпендикул€р справа

            if (!Physics2D.Raycast(transform.position, altDir1, 0.5f, wallMask))
                transform.position += altDir1.normalized * (Velocity * Time.deltaTime);
            else if (!Physics2D.Raycast(transform.position, altDir2, 0.5f, wallMask))
                transform.position += altDir2.normalized * (Velocity * Time.deltaTime);
        }
    }
}
