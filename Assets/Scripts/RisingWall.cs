using System.Collections;
using UnityEngine;

// Ground 가장자리에서 솟아올라 일정 시간 유지된 뒤, 솟아오른 모션을 거꾸로 재생하며 내려가고 사라지는 벽.
// 움직이는 동안 흙먼지 이펙트를 낸다.
public class RisingWall : MonoBehaviour
{
    [Tooltip("벽 몸체 (BoxCollider, 키네마틱 Rigidbody 포함)")]
    public Transform body;

    [Tooltip("벽이 움직이는 동안 재생할 흙먼지 이펙트")]
    public ParticleSystem dust;

    public float length = 10.6f;
    public float thickness = 0.3f;
    public float height = 1.5f;

    [Tooltip("솟아오르는/내려가는 데 걸리는 시간 (초)")]
    public float moveDuration = 1.5f;

    [Tooltip("다 솟아오른 뒤 유지되는 시간 (초)")]
    public float holdDuration = 10f;

    public int Side { get; set; }

    IEnumerator Start()
    {
        SetHeight(0f);
        yield return Animate(0f, 1f);
        yield return new WaitForSeconds(holdDuration);
        yield return Animate(1f, 0f);

        body.gameObject.SetActive(false);
        // 남은 흙먼지 입자가 사라질 때까지 기다렸다가 삭제
        Destroy(gameObject, dust.main.startLifetime.constantMax);
    }

    IEnumerator Animate(float from, float to)
    {
        dust.Play();

        float t = 0f;
        while (t < moveDuration)
        {
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
            SetHeight(Mathf.SmoothStep(from, to, t / moveDuration));
        }
        SetHeight(to);

        dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // 바닥면은 Ground 높이에 고정한 채 위쪽으로 자라게 한다
    void SetHeight(float k)
    {
        float h = Mathf.Max(0.001f, height * k);
        body.localScale = new Vector3(length, h, thickness);
        body.localPosition = new Vector3(0f, h * 0.5f, 0f);
    }
}
