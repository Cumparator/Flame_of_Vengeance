using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject banditPrefab;
    [SerializeField] private GameObject rangeEnemyPrefab;
    [SerializeField] private Transform player;

    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private int banditsPerSpawn = 3;
    [SerializeField] private int rangeEnemyPerSpawn = 2;
    [SerializeField] private float spawnCooldown = 5f;

    private float nextSpawnTime = 2f;

    // Ограничения по координатам
    private float minX = -40f;
    private float maxX = 46f;
    private float minY = -19f;
    private float maxY = 22f;

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
            Vector3 spawnPosition = GetPointInBounds();
            Instantiate(banditPrefab, spawnPosition, Quaternion.identity);
        }

        for (int i = 0; i < rangeEnemyPerSpawn; i++)
        {
            Vector3 spawnPosition = GetPointInBounds();
            Instantiate(rangeEnemyPrefab, spawnPosition, Quaternion.identity);
        }

        if (spawnCooldown >= 1)
        {
            spawnCooldown *= 0.9f;
        }
    }

    Vector3 GetPointInBounds()
    {
        // Случайная позиция на радиусе вокруг игрока
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = new Vector3(
            player.position.x + randomDirection.x * spawnRadius,
            player.position.y + randomDirection.y * spawnRadius,
            0f
        );

        // Ограничиваем позицию в допустимых границах
        spawnPosition.x = Mathf.Clamp(spawnPosition.x, minX, maxX);
        spawnPosition.y = Mathf.Clamp(spawnPosition.y, minY, maxY);

        return spawnPosition;
    }
}
