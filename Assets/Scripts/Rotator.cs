using UnityEngine;

// 오브젝트를 Y축 기준으로 회전시킨다.
public class Rotator : MonoBehaviour
{
    [Tooltip("초당 회전 각도 (도)")]
    public float rotationSpeed = 90f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null) return;

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 키네마틱이면 회전으로 주변 물체를 밀어내도록 MoveRotation으로 돌린다
        if (rb.isKinematic)
        {
            rb.MoveRotation(Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f) * rb.rotation);
            return;
        }

        // 떨어지는 중(일반 Rigidbody)이면 물리 엔진의 각속도로 회전시킨다
        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.y = rotationSpeed * Mathf.Deg2Rad;
        rb.angularVelocity = angularVelocity;
    }
}
