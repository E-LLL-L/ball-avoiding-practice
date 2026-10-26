using UnityEngine;

// 오브젝트가 Main Camera 화면 안에 보이는지 판단한다.
public static class CameraVisibility
{
    static readonly Plane[] planes = new Plane[6];

    public static bool IsVisible(Renderer renderer)
    {
        Camera cam = Camera.main;
        if (cam == null || renderer == null) return true;

        GeometryUtility.CalculateFrustumPlanes(cam, planes);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
