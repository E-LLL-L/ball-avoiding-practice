using UnityEngine;

// 일정 간격마다 공중의 랜덤 위치에 Sphere를 생성한다.
public class SphereSpawner : MonoBehaviour
{
    [Tooltip("생성할 Sphere 프리팹 (Rigidbody 포함)")]
    public GameObject spherePrefab;

    [Tooltip("생성 간격 (초)")]
    public float spawnInterval = 3f;

    [Tooltip("생성 높이")]
    public float spawnHeight = 10f;

    [Tooltip("X/Z 랜덤 범위 (-range ~ +range)")]
    public float spawnRange = 4.5f;

    float timer;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            Spawn();
        }
    }

    void Spawn()
    {
        Vector3 position = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            spawnHeight,
            Random.Range(-spawnRange, spawnRange));
        Instantiate(spherePrefab, position, Quaternion.identity);
    }
}
