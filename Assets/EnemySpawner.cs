using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject banditPrefab;
    public GameObject rangeEnemyPrefab;
    public Transform player;

    public float spawnRadius = 20f;
    public int banditsPerSpawn = 3;
    public int rangeEnemyPerSpawn = 2;
    public float spawnCooldown = 5f;

    private float nextSpawnTime = 10f;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemies();
            nextSpawnTime = Time.time + spawnCooldown;
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < banditsPerSpawn; i++)
        {
            Vector3 spawnPosition = GetPointOnSpawnRadius();
            Instantiate(banditPrefab, spawnPosition, Quaternion.identity);
        }

        for (int i = 0; i < rangeEnemyPerSpawn; i++)
        {
            Vector3 spawnPosition = GetPointOnSpawnRadius();
            Instantiate(rangeEnemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetPointOnSpawnRadius()
    {
        // Случайное направление по XY
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Позиция на радиусе
        Vector3 spawnPosition = new Vector3(
            player.position.x + randomDirection.x * spawnRadius,
            player.position.y + randomDirection.y * spawnRadius,
            0f // по Z ставим 0, если надо
        );

        return spawnPosition;
    }
}
