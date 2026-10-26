using System.Collections.Generic;
using UnityEngine;

// 장애물 Sphere. Ground 밖으로 떨어진 뒤 화면 밖으로 나가면 삭제되고, Game Over 시 그 자리에 멈춘다.
[RequireComponent(typeof(Rigidbody))]
public class ObstacleSphere : MonoBehaviour
{
    static readonly List<ObstacleSphere> activeSpheres = new List<ObstacleSphere>();

    [Tooltip("이 높이보다 아래로 내려가면 Ground 밖으로 떨어진 것으로 본다")]
    public float fallenHeight = -1f;

    Rigidbody rb;
    Renderer rend;
    bool isFrozen;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        activeSpheres.Add(this);
    }

    void OnDisable()
    {
        activeSpheres.Remove(this);
    }

    void Update()
    {
        if (isFrozen) return;

        // Ground 위에 있는 동안은 유지, 떨어진 뒤에도 화면에 보이는 동안은 유지
        if (transform.position.y < fallenHeight && !CameraVisibility.IsVisible(rend))
            Destroy(gameObject);
    }

    public void Freeze()
    {
        isFrozen = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public static void FreezeAll()
    {
        for (int i = activeSpheres.Count - 1; i >= 0; i--)
            activeSpheres[i].Freeze();
    }
}
