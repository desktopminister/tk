using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TKBattleEnding : MonoBehaviour
{
    public const string VictoryScene = "Assets/Scenes/Victory.unity";
    public const string DefeatScene = "Assets/Scenes/Defeat.unity";
    const string DefaultBattlefield = "Assets/Scenes/Demo.unity";

    [SerializeField] Text elapsed;
    [SerializeField] Text kills;
    [SerializeField] Text level;
    [SerializeField] Button retry;
    [SerializeField] CanvasGroup curtain;
    [SerializeField] TKGameAudio audioPrefab;

    static (string battlefield, int seconds, int kills, int level) report;
    static string pendingDeployment;
    bool leaving;
    float enteredAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession()
    {
        report = default;
        pendingDeployment = null;
    }

    public static void Record(GameManager game)
    {
        report = (game.gameObject.scene.path, Mathf.FloorToInt(game.gameTime), game.kill, game.level);
    }

    public static bool ConsumeDeployment(string battlefield)
    {
        if (pendingDeployment != battlefield) return false;
        pendingDeployment = null;
        return true;
    }

    void Start()
    {
        Time.timeScale = 1f;
        Instantiate(audioPrefab, transform).PlayEnding(gameObject.scene.path == VictoryScene);
        if (!string.IsNullOrEmpty(report.battlefield))
        {
            elapsed.text = $"{report.seconds / 60:00}:{report.seconds % 60:00}";
            kills.text = report.kills.ToString();
            level.text = report.level.ToString();
        }
        enteredAt = Time.unscaledTime;
        curtain.alpha = 0f;
        retry.Select();
    }

    void LateUpdate()
    {
        if (curtain.alpha < 1f)
            curtain.alpha = Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - enteredAt) / 0.4f);

        if (!leaving && EventSystem.current && !EventSystem.current.currentSelectedGameObject)
            retry.Select();
    }

    public void Retry() => ReturnToBattlefield(true);
    public void ReturnToTitle() => ReturnToBattlefield(false);

    void ReturnToBattlefield(bool deploy)
    {
        if (leaving) return;
        leaving = true;
        string battlefield = string.IsNullOrEmpty(report.battlefield) ? DefaultBattlefield : report.battlefield;
        pendingDeployment = deploy ? battlefield : null;
        report = default;
        SceneManager.LoadScene(SceneUtility.GetBuildIndexByScenePath(battlefield));
    }
}
