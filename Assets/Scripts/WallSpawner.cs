using System.Collections.Generic;
using UnityEngine;

// 플레이타임 기준 일정 간격마다 Ground 네 변 중 한 곳에 벽을 솟아오르게 한다.
public class WallSpawner : MonoBehaviour
{
    [Tooltip("생성할 벽 프리팹")]
    public RisingWall wallPrefab;

    [Tooltip("벽 생성 간격 (플레이타임 기준 초)")]
    public float spawnInterval = 10f;

    [Tooltip("Ground 중심에서 가장자리까지 거리")]
    public float groundHalfSize = 5f;

    float nextSpawnTime;
    readonly List<RisingWall> walls = new List<RisingWall>();

    void Start()
    {
        nextSpawnTime = spawnInterval;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        if (gm.SurvivalTime >= nextSpawnTime)
        {
            nextSpawnTime += spawnInterval;
            Spawn();
        }
    }

    void Spawn()
    {
        walls.RemoveAll(w => w == null);

        // 이미 벽이 서 있는 변은 제외하고 고른다
        var freeSides = new List<int> { 0, 1, 2, 3 };
        foreach (RisingWall w in walls) freeSides.Remove(w.Side);
        if (freeSides.Count == 0) return;

        int side = freeSides[Random.Range(0, freeSides.Count)];
        float offset = groundHalfSize + wallPrefab.thickness * 0.5f;

        Vector3 position;
        Quaternion rotation;
        switch (side)
        {
            case 0: position = new Vector3(0f, 0f, offset); rotation = Quaternion.identity; break;            // 북(먼 쪽)
            case 1: position = new Vector3(0f, 0f, -offset); rotation = Quaternion.identity; break;           // 남(카메라 쪽)
            case 2: position = new Vector3(offset, 0f, 0f); rotation = Quaternion.Euler(0f, 90f, 0f); break;  // 동
            default: position = new Vector3(-offset, 0f, 0f); rotation = Quaternion.Euler(0f, 90f, 0f); break; // 서
        }

        RisingWall wall = Instantiate(wallPrefab, position, rotation);
        wall.Side = side;
        walls.Add(wall);
    }
}
