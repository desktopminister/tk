using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class CameraView
{
    // Keeps spawned or moved enemies fully outside the screen
    public const float EnemySpawnMargin = 0.5f;
    // Enemies farther outside the screen than this are moved back near the player
    public const float EnemyRepositionMargin = 1f;

    public static bool TryGetWorldRect(float planeZ, float margin, out Rect rect)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            rect = default;
            return false;
        }

        Transform camTransform = cam.transform;
        float depth = planeZ - camTransform.position.z;
        float halfHeight = GetRenderedHalfHeight(cam, depth, out float aspect);
        Vector3 right = camTransform.right * (halfHeight * aspect);
        Vector3 up = camTransform.up * halfHeight;
        Vector3 center = camTransform.position + camTransform.forward * depth;
        float extentX = Mathf.Abs(right.x) + Mathf.Abs(up.x) + margin;
        float extentY = Mathf.Abs(right.y) + Mathf.Abs(up.y) + margin;
        rect = Rect.MinMaxRect(center.x - extentX, center.y - extentY, center.x + extentX, center.y + extentY);
        return true;
    }

    static float GetRenderedHalfHeight(Camera cam, float depth, out float aspect)
    {
        aspect = cam.aspect;
        if (!cam.orthographic)
            return Mathf.Abs(depth) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Pixel Perfect Camera replaces the orthographic size right before rendering, based on the screen resolution,
        // so the size Cinemachine writes to the camera is not what ends up on screen.
        if (!cam.TryGetComponent(out PixelPerfectCamera pixelPerfect) || !pixelPerfect.isActiveAndEnabled)
            return cam.orthographicSize;

        if (pixelPerfect.cropFrame != PixelPerfectCamera.CropFrame.None)
            return pixelPerfect.orthographicSize;

        // Same calculation as PixelPerfectCamera without cropping, using this frame's resolution
        RenderTexture target = cam.targetTexture;
        int screenWidth = target != null ? target.width : Screen.width;
        int screenHeight = target != null ? target.height : Screen.height;
        if (screenWidth <= 0 || screenHeight <= 0)
            return pixelPerfect.orthographicSize;

        int zoom = Mathf.Max(1, Mathf.Min(screenHeight / pixelPerfect.refResolutionY, screenWidth / pixelPerfect.refResolutionX));
        aspect = (float)screenWidth / screenHeight;
        if (pixelPerfect.gridSnapping == PixelPerfectCamera.GridSnapping.UpscaleRenderTexture && zoom > 1)
            return screenHeight / zoom / 2 * 2 * 0.5f / pixelPerfect.assetsPPU;
        return screenHeight * 0.5f / (zoom * pixelPerfect.assetsPPU);
    }

    public static Vector2 PointOnEdge(Rect rect, Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;
        direction.Normalize();

        Vector2 half = rect.size * 0.5f;
        float scaleX = direction.x != 0f ? half.x / Mathf.Abs(direction.x) : float.PositiveInfinity;
        float scaleY = direction.y != 0f ? half.y / Mathf.Abs(direction.y) : float.PositiveInfinity;
        return rect.center + direction * Mathf.Min(scaleX, scaleY);
    }
}
