using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 플레이어를 이동시키고, Sphere에 닿으면 Game Over 처리 후 폭발과 함께 날아간다.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("이동 속도 (단위/초)")]
    public float moveSpeed = 5f;

    [Header("Game Over 연출")]
    [Tooltip("충돌 시 생성할 폭발 이펙트")]
    public GameObject explosionPrefab;

    [Tooltip("카메라 쪽으로 날아올 때 카메라에 닿기까지 걸리는 시간 (초)")]
    public float timeToCamera = 0.9f;

    [Tooltip("카메라 반대쪽으로 날아갈 때 수평 속도")]
    public float awaySpeed = 12f;

    [Tooltip("카메라 반대쪽으로 날아갈 때 위쪽 속도")]
    public float awayUpSpeed = 6f;

    [Tooltip("날아갈 때 회전 속도 (라디안/초)")]
    public float spinSpeed = 20f;

    [Tooltip("이 높이보다 아래로 떨어지면 Ground 밖으로 떨어진 것으로 보고 Game Over")]
    public float fallGameOverHeight = -1f;

    [Header("추락 Game Over 연출")]
    [Tooltip("화면 아래쪽 폭발 위치의 세로 화면 좌표 (0 = 화면 아래 끝, 음수 = 화면 밖)")]
    public float fallBlastViewportY = -0.03f;

    [Tooltip("카메라에서 폭발 위치까지의 거리")]
    public float fallBlastDistance = 12f;

    [Tooltip("폭발 지점에서 카메라에 닿기까지 걸리는 시간 (초). 길수록 화면 위쪽으로 높이 솟았다가 날아온다")]
    public float fallTimeToCamera = 1.2f;

    [Tooltip("화면 밖으로 나가지 않더라도 이 시간이 지나면 연출 시작 (초)")]
    public float fallEffectMaxWait = 5f;

    Rigidbody rb;
    Renderer rend;
    Vector3 moveInput;
    bool isGameOver;
    bool isFallingOff;
    float launchTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (isGameOver)
        {
            float flightTime = Time.time - launchTime;

            // 추락 중인 캡슐이 화면 밖으로 나가면 화면 아래쪽에서 폭발하며 날아온다
            if (isFallingOff)
            {
                if (!CameraVisibility.IsVisible(rend) || flightTime > fallEffectMaxWait)
                    FallOffExplode();
                return;
            }

            // 날아간 캡슐이 화면 밖으로 나가면 정리
            if ((flightTime > 0.3f && !CameraVisibility.IsVisible(rend)) || flightTime > 6f)
                gameObject.SetActive(false);
            return;
        }

        // Ground 밖으로 떨어지면 Game Over. 연출은 화면 밖으로 나간 뒤에 시작한다
        if (transform.position.y < fallGameOverHeight)
        {
            TriggerGameOver();
            isFallingOff = true;
            return;
        }

        moveInput = Vector3.zero;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) moveInput.z += 1f;
        if (keyboard.sKey.isPressed) moveInput.z -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        moveInput = Vector3.ClampMagnitude(moveInput, 1f);
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        Vector3 velocity = moveInput * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isGameOver) return;
        if (!collision.gameObject.CompareTag("Obstacle")) return;

        TriggerGameOver();
        Explode();
    }

    void TriggerGameOver()
    {
        isGameOver = true;
        moveInput = Vector3.zero;
        launchTime = Time.time;

        Debug.Log("Game Over");
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }

    // Sphere에 맞았을 때: 그 자리에서 폭발하고, 카메라 쪽 또는 반대쪽으로 날아간다
    void Explode()
    {
        SpawnExplosion(transform.position);

        Camera cam = Camera.main;
        if (cam != null && Random.value < 0.5f)
        {
            Launch(cam, timeToCamera);
        }
        else
        {
            // 카메라 시점 기준 먼 곳으로 날아간다
            Vector3 away = cam != null ? cam.transform.forward : Vector3.forward;
            away.y = 0f;
            away.Normalize();
            PrepareFlight();
            rb.linearVelocity = away * awaySpeed + Vector3.up * awayUpSpeed;
            launchTime = Time.time;
        }
    }

    // 추락해 화면 밖으로 나갔을 때: 화면 아래 바로 밖에서 폭발하고, 카메라 쪽으로 날아온다
    void FallOffExplode()
    {
        isFallingOff = false;

        Camera cam = Camera.main;
        if (cam == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 캡슐이 사라진 가로 위치의 화면 아래쪽 (위쪽 절반 정도만 화면에 보이는 위치)
        float viewX = Mathf.Clamp(cam.WorldToViewportPoint(transform.position).x, 0.15f, 0.85f);
        Vector3 blastPosition = cam.ViewportToWorldPoint(new Vector3(viewX, fallBlastViewportY, fallBlastDistance));

        SpawnExplosion(blastPosition);

        rb.position = blastPosition;
        transform.position = blastPosition;
        Launch(cam, fallTimeToCamera);
    }

    void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab == null) return;

        GameObject fx = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(fx, 5f);
    }

    // 카메라를 향해 정면으로 날아온다 (duration 뒤 카메라 위치를 지나는 포물선)
    void Launch(Camera cam, float duration)
    {
        PrepareFlight();
        Vector3 toCamera = cam.transform.position - rb.position;
        rb.linearVelocity = toCamera / duration - 0.5f * Physics.gravity * duration;
        launchTime = Time.time;
    }

    // 날아가는 동안 다른 물체에 걸리지 않도록 충돌을 끄고, 빙글빙글 돌게 한다
    void PrepareFlight()
    {
        GetComponent<Collider>().enabled = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.maxAngularVelocity = spinSpeed * 2f;
        rb.angularVelocity = Random.onUnitSphere * spinSpeed;
    }
}
