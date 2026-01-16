using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform arenaCenter;
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 10f;
    public float minSpawnDistance = 4f;
    public float spawnInterval = 1.5f;
    public int maxEnemies = 50;

    [Header("Arena Settings")]
    public float arenaRadius = 20f;
    public float edgePadding = 1f;

    float spawnTimer;
    int currentEnemies;

    void Update()
    {
        if (!player || !arenaCenter || !enemyPrefab)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && currentEnemies < maxEnemies)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = GetValidSpawnPosition();

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemies++;

        enemy.AddComponent<SpawnedEnemy>().spawner = this;
    }

    Vector3 GetValidSpawnPosition()
    {
        // Random direction around player
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

        // Random distance between min and max
        float distance = Random.Range(minSpawnDistance, spawnRadius);

        Vector3 desiredPos = player.position + dir * distance;

        // Clamp inside arena
        Vector3 toSpawn = desiredPos - arenaCenter.position;
        float maxArenaDistance = arenaRadius - edgePadding;

        if (toSpawn.magnitude > maxArenaDistance)
        {
            toSpawn = toSpawn.normalized * maxArenaDistance;
        }

        Vector3 finalPos = arenaCenter.position + toSpawn;
        finalPos.y = player.position.y;

        // Safety: enforce minimum distance after clamping
        Vector3 fromPlayer = finalPos - player.position;
        float playerDistance = fromPlayer.magnitude;

        if (playerDistance < minSpawnDistance)
        {
            finalPos = player.position + fromPlayer.normalized * minSpawnDistance;
            finalPos.y = player.position.y;
        }

        return finalPos;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemies--;
    }

    void OnDrawGizmosSelected()
    {
        if (!arenaCenter || !player) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(arenaCenter.position, arenaRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, spawnRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
    }
}
