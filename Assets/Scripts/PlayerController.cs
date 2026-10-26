using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 플레이어를 이동시키고, Sphere에 닿으면 Game Over 처리한다.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("이동 속도 (단위/초)")]
    public float moveSpeed = 5f;

    Rigidbody rb;
    Vector3 moveInput;
    bool isGameOver;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (isGameOver) return;

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

        isGameOver = true;
        moveInput = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("Game Over");
        if (GameManager.Instance != null) GameManager.Instance.GameOver();
    }
}
