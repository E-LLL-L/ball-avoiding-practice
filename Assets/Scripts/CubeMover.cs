using UnityEngine;

// Cube를 랜덤한 방향으로 천천히 이동시킨다.
// 키네마틱 Rigidbody로 움직여서 공과 플레이어는 밀어내지만, 공과 플레이어에게는 밀리지 않는다.
// Ground 가장자리에 닿으면 대부분 방향을 바꾸지만, 가끔 하나는 그대로 나아가 떨어진다.
[RequireComponent(typeof(Rigidbody))]
public class CubeMover : MonoBehaviour
{
    [Tooltip("이동 속도 (단위/초)")]
    public float moveSpeed = 0.8f;

    [Tooltip("방향을 살짝 바꾸는 간격 (최소~최대 초)")]
    public Vector2 turnInterval = new Vector2(3f, 6f);

    [Tooltip("한 번에 바꾸는 최대 각도")]
    public float maxTurnAngle = 45f;

    [Tooltip("중심에서 이 거리를 넘으면 가장자리로 판단")]
    public float edgeLimit = 4.3f;

    [Tooltip("Ground 중심에서 끝까지 거리. Cube 중심이 이 밖으로 나가면 떨어진다")]
    public float groundHalfSize = 5f;

    [Tooltip("이 높이보다 아래로 내려간 뒤 화면 밖으로 나가면 삭제")]
    public float removeHeight = -1f;

    [Tooltip("앞쪽 장애물을 미리 감지하는 여유 거리")]
    public float lookAhead = 0.05f;

    Rigidbody rb;
    Renderer rend;
    Collider col;
    Vector3 direction;
    float nextTurnTime;
    bool isWandering;
    bool isFalling;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        rb.isKinematic = true;
    }

    void Start()
    {
        float angle = Random.Range(0f, 360f);
        direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        ScheduleTurn();
    }

    void Update()
    {
        if (isFalling && transform.position.y < removeHeight && !CameraVisibility.IsVisible(rend))
            Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (isFalling) return;

        Vector3 p = rb.position;
        if (Mathf.Abs(p.x) > groundHalfSize || Mathf.Abs(p.z) > groundHalfSize)
        {
            StartFalling();
            return;
        }

        if (!isWandering && Time.time >= nextTurnTime)
        {
            direction = Quaternion.Euler(0f, Random.Range(-maxTurnAngle, maxTurnAngle), 0f) * direction;
            ScheduleTurn();
        }

        HandleEdges();

        float step = moveSpeed * Time.fixedDeltaTime;
        BounceOffBlockers(step);

        rb.MovePosition(rb.position + direction * step);
    }

    void HandleEdges()
    {
        if (isWandering) return;

        Vector3 p = rb.position;
        bool outX = (p.x > edgeLimit && direction.x > 0f) || (p.x < -edgeLimit && direction.x < 0f);
        bool outZ = (p.z > edgeLimit && direction.z > 0f) || (p.z < -edgeLimit && direction.z < 0f);
        if (!outX && !outZ) return;

        // 가끔 하나는 방향을 바꾸지 않고 Ground 밖으로 나아간다
        if (CubeSpawner.Instance != null && CubeSpawner.Instance.TryStartWander(this))
        {
            isWandering = true;
            return;
        }

        if (outX) direction.x = -direction.x;
        if (outZ) direction.z = -direction.z;
    }

    // 진행 방향 앞에 벽, 다른 Cube, 멈춘 공처럼 밀 수 없는 물체가 있으면 튕겨 방향을 바꾼다.
    // 움직일 수 있는 물체(공, 플레이어)는 무시하고 그대로 밀고 나아간다.
    void BounceOffBlockers(float step)
    {
        Vector3 halfExtents = new Vector3(0.45f, 0.4f, 0.45f);
        Vector3 center = rb.position + Vector3.up * 0.05f;
        RaycastHit[] hits = Physics.BoxCastAll(center, halfExtents, direction, rb.rotation, step + lookAhead, ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == col) continue;
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic) continue;   // 밀어낼 수 있는 물체
            if (hit.normal.y > 0.5f) continue;                                    // 바닥

            Vector3 normal = hit.normal;
            normal.y = 0f;
            if (normal.sqrMagnitude < 0.0001f) normal = -direction;
            normal.Normalize();

            if (Vector3.Dot(direction, normal) < 0f)
                direction = Vector3.Reflect(direction, normal);

            // 벽에 막히면 떨어지려던 시도는 끝
            if (isWandering && hit.collider.CompareTag("Wall"))
                StopWandering();
            return;
        }
    }

    void StartFalling()
    {
        isFalling = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = direction * moveSpeed;
    }

    void StopWandering()
    {
        isWandering = false;
        if (CubeSpawner.Instance != null) CubeSpawner.Instance.EndWander(this);
    }

    void ScheduleTurn()
    {
        nextTurnTime = Time.time + Random.Range(turnInterval.x, turnInterval.y);
    }

    void OnDestroy()
    {
        if (isWandering && CubeSpawner.Instance != null) CubeSpawner.Instance.EndWander(this);
    }
}
