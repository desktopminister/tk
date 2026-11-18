using UnityEngine;
using UnityEngine.UI;

// One UI mesh for the flecks of gold or slanting rain over the landscape.
public class TKEndingWeather : MaskableGraphic
{
    [SerializeField] bool rain;
    int frame = -1;

    void Update()
    {
        int next = Mathf.FloorToInt(Time.unscaledTime * 16f);
        if (next == frame) return;
        frame = next;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect bounds = rectTransform.rect;
        float unit = bounds.height / 540f;
        float time = Mathf.Max(0, frame) / 16f;
        for (int i = 0; i < (rain ? 48 : 26); i++)
        {
            float phase = Mathf.Repeat(i * 0.618f + time * (rain ? 0.65f : 0.085f) * (0.85f + (i % 5) * 0.1f), 1f);
            float wave = Mathf.Sin(phase * 8f + i);
            float x = Mathf.Repeat(i * 0.379f - phase * (rain ? 0.08f : 0.12f) + (rain ? 0f : wave * 0.018f), 1f);
            Vector2 center = new Vector2(bounds.xMin + x * bounds.width, bounds.yMax - phase * bounds.height);
            Vector2 along = unit * (rain ? new Vector2(1.6f, 5f + i % 8) : new Vector2(wave * 2f, 1.5f));
            Vector2 across = Vector2.Perpendicular(along).normalized * unit * (rain ? 0.35f : 0.85f);
            Color tint = color;
            tint.a *= Mathf.Sin(phase * Mathf.PI) * (0.45f + 0.12f * (i % 4));
            int start = mesh.currentVertCount;
            mesh.AddVert(center - along - across, tint, Vector2.zero);
            mesh.AddVert(center + along - across, tint, Vector2.zero);
            mesh.AddVert(center + along + across, tint, Vector2.zero);
            mesh.AddVert(center - along + across, tint, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
