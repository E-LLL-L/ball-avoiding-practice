using System.Collections;
using UnityEngine;

// 플레이타임이 일정 시간 지날 때마다 바닥에서 새 Cube를 솟아오르게 한다.
// 또한 Ground 밖으로 나가 떨어지는 Cube가 "가끔 하나"만 생기도록 관리한다.
public class CubeSpawner : MonoBehaviour
{
    public static CubeSpawner Instance { get; private set; }

    [Tooltip("생성할 Cube 프리팹 (Rigidbody, CubeMover 포함)")]
    public GameObject cubePrefab;

    [Tooltip("새 Cube 생성 간격 (플레이타임 기준 초)")]
    public float spawnInterval = 20f;

    [Tooltip("생성 위치 X/Z 랜덤 범위 (-range ~ +range)")]
    public float spawnRange = 3.8f;

    [Tooltip("바닥에서 솟아오르는 데 걸리는 시간 (초)")]
    public float riseDuration = 1.2f;

    [Header("Ground 밖으로 떨어지는 Cube")]
    [Tooltip("가장자리에 닿았을 때 밖으로 나아갈 확률")]
    [Range(0f, 1f)] public float wanderChance = 0.25f;

    [Tooltip("게임 시작 후 이 시간 동안은 떨어지는 Cube가 생기지 않음 (초)")]
    public float firstWanderDelay = 15f;

    [Tooltip("한 Cube가 떨어진 뒤 다음 Cube가 떨어질 수 있을 때까지 대기 (초)")]
    public float wanderCooldown = 20f;

    float nextSpawnTime;
    float nextWanderTime;
    CubeMover wanderer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        nextSpawnTime = spawnInterval;
        nextWanderTime = Time.time + firstWanderDelay;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return;

        if (gm.SurvivalTime >= nextSpawnTime)
        {
            nextSpawnTime += spawnInterval;
            StartCoroutine(SpawnCube());
        }
    }

    IEnumerator SpawnCube()
    {
        Vector3 top = FindSpawnPosition();
        Vector3 bottom = top + Vector3.down * 1f;

        GameObject cube = Instantiate(cubePrefab, bottom, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        SetRandomColor(cube.GetComponent<Renderer>());

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        CubeMover mover = cube.GetComponent<CubeMover>();
        mover.enabled = false;
        rb.isKinematic = true;

        float t = 0f;
        while (t < riseDuration)
        {
            yield return new WaitForFixedUpdate();
            if (cube == null) yield break;
            t += Time.fixedDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / riseDuration);
            rb.MovePosition(Vector3.Lerp(bottom, top, k));
        }

        mover.enabled = true;
    }

    Vector3 FindSpawnPosition()
    {
        Vector3 candidate = Vector3.zero;
        for (int i = 0; i < 30; i++)
        {
            candidate = new Vector3(Random.Range(-spawnRange, spawnRange), 0.5f, Random.Range(-spawnRange, spawnRange));
            // 플레이어, 다른 Cube, Sphere와 겹치지 않는 자리 (Ground 표면은 닿지 않는 높이로 검사)
            if (!Physics.CheckSphere(candidate + Vector3.up * 0.6f, 0.9f))
                return candidate;
        }
        return candidate;
    }

    void SetRandomColor(Renderer rend)
    {
        Color color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        rend.SetPropertyBlock(block);
    }

    public bool TryStartWander(CubeMover cube)
    {
        if (wanderer != null || Time.time < nextWanderTime) return false;
        if (Random.value > wanderChance) return false;

        wanderer = cube;
        return true;
    }

    public void EndWander(CubeMover cube)
    {
        if (wanderer != cube) return;

        wanderer = null;
        nextWanderTime = Time.time + wanderCooldown;
    }
}
