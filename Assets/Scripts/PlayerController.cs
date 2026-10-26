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

    Rigidbody rb;
    Renderer rend;
    Vector3 moveInput;
    bool isGameOver;
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
            // 날아간 캡슐이 화면 밖으로 나가면 정리
            float flightTime = Time.time - launchTime;
            if ((flightTime > 0.3f && !CameraVisibility.IsVisible(rend)) || flightTime > 6f)
                gameObject.SetActive(false);
            return;
        }

        // Ground 밖으로 떨어지면 Game Over (폭발 없이 그대로 떨어진다)
        if (transform.position.y < fallGameOverHeight)
        {
            TriggerGameOver();
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

    void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 5f);
        }

        // 날아가는 동안 다른 물체에 걸리지 않도록 충돌을 끈다
        GetComponent<Collider>().enabled = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.maxAngularVelocity = spinSpeed * 2f;

        Camera cam = Camera.main;
        Vector3 velocity;
        if (cam != null && Random.value < 0.5f)
        {
            // 카메라를 향해 정면으로 날아온다 (timeToCamera 뒤 카메라 위치를 지나는 포물선)
            Vector3 toCamera = cam.transform.position - transform.position;
            velocity = toCamera / timeToCamera - 0.5f * Physics.gravity * timeToCamera;
        }
        else
        {
            // 카메라 시점 기준 먼 곳으로 날아간다
            Vector3 away = cam != null ? cam.transform.forward : Vector3.forward;
            away.y = 0f;
            away.Normalize();
            velocity = away * awaySpeed + Vector3.up * awayUpSpeed;
        }

        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.onUnitSphere * spinSpeed;
        launchTime = Time.time;
    }
}
