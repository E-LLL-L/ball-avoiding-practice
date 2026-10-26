using UnityEngine;

// 오브젝트를 Y축 기준으로 회전시킨다.
public class Rotator : MonoBehaviour
{
    [Tooltip("초당 회전 각도 (도)")]
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
