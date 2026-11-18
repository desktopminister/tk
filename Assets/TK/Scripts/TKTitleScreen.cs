using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class TKTitleScreen : MonoBehaviour
{
    [SerializeField] RectTransform lettering;
    [SerializeField] RectTransform landscape;
    [SerializeField] RectTransform kongming;
    [SerializeField] Button deploy;
    [SerializeField] TKGameAudio audioPrefab;

    readonly Image[] embers = new Image[18];
    int lastFrame = -1;

    void OnEnable() => FitLettering();
    void OnRectTransformDimensionsChange() => FitLettering();

    void FitLettering()
    {
        if (!lettering) return;
        Rect bounds = ((RectTransform)transform).rect;
        float scale = Mathf.Min(bounds.width / 960f, bounds.height / 540f);
        if (scale > 0f) lettering.localScale = Vector3.one * scale;
    }

    void Start()
    {
        if (!Application.IsPlaying(gameObject)) return;
        // The title is hidden on deployment. Its sibling keeps playing through the battle.
        var sound = Instantiate(audioPrefab, transform.parent);
        sound.PlayTitle();
        if (TKBattleEnding.ConsumeDeployment(gameObject.scene.path))
        {
            deploy.onClick.Invoke();
            return;
        }
        for (int i = 0; i < embers.Length; i++)
        {
            var spark = TKUIRoot.NewImage("Ember", landscape, null, Color.white, false);
            spark.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -22f);
            embers[i] = spark;
        }
        deploy.Select();
    }

    void LateUpdate()
    {
        if (!Application.IsPlaying(gameObject)) return;
        // A click on the landscape can clear uGUI focus. Keep Submit available.
        if (EventSystem.current && !EventSystem.current.currentSelectedGameObject)
            deploy.Select();

        int frame = Mathf.FloorToInt(Time.unscaledTime * 12f);
        if (frame == lastFrame) return;
        lastFrame = frame;
        float time = frame / 12f;
        kongming.localScale = new Vector3(-1f, 1f + 0.007f * Mathf.Sin(time * 2f), 1f);

        float pixel = landscape.rect.width / 960f;
        for (int i = 0; i < embers.Length; i++)
        {
            float phase = Mathf.Repeat(time * (0.095f + 0.012f * (i % 4)) + i * 0.618f, 1f);
            float x = 0.55f + Mathf.Repeat(i * 0.173f, 0.46f) - 0.085f * phase;
            float y = 0.12f + Mathf.Repeat(i * 0.071f, 0.32f) + 0.19f * phase;
            float sway = 0.004f * Mathf.Sin(phase * 9f + i);
            var spark = embers[i];
            var rt = spark.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(x + sway, y);
            rt.sizeDelta = pixel * new Vector2(1f, 1f + i % 3);
            spark.color = new Color(1f, 0.58f + 0.10f * (i % 3), 0.32f,
                0.75f * Mathf.Sin(phase * Mathf.PI));
        }
    }
}
