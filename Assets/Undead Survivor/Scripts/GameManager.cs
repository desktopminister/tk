using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("#Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxPlayTime = 60f;
    [Tooltip("보스전 중에는 제한 시간이 끝나도 자동 승리하지 않음 (BossDirector 가 설정)")]
    public bool bossFightActive;
    [SerializeField] bool useEndingScenes;
    [Header("#Player Info")]
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = {10, 30, 60, 100, 150, 210, 280, 360, 450, 550};
    [Header("#Managers")]
    public PoolManager poolManager;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;
    int damageBatchDepth;
    int pendingLevelUps;
    bool ending;
    void Awake()
    {
        instance = this;
    }

    public void GameStart()
    {
        health = maxHealth;
        uiLevelUp.Select(0);
        Resume();
        TKGameAudio.BeginBattle();
    }

    public void GameOver() => EndBattle(false);
    public void GameWin() => EndBattle(true);

    void EndBattle(bool victory)
    {
        if (ending) return;
        ending = true;
        isLive = false;
        TKGameAudio.EndBattle();
        if (useEndingScenes) TKBattleEnding.Record(this);
        if (victory) enemyCleaner.SetActive(true);
        StartCoroutine(ShowEnding(victory));
    }

    IEnumerator ShowEnding(bool victory)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Stop();
        if (useEndingScenes)
        {
            string scene = victory ? TKBattleEnding.VictoryScene : TKBattleEnding.DefeatScene;
            SceneManager.LoadScene(SceneUtility.GetBuildIndexByScenePath(scene));
            yield break;
        }

        uiResult.gameObject.SetActive(true);
        if (victory) uiResult.Win();
        else uiResult.Lose();
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (!isLive)
            return;
        gameTime += Time.deltaTime;
        if (gameTime >= maxPlayTime)
        {
            gameTime = maxPlayTime;
            if (!bossFightActive)
                GameWin();
        }
    }
    public void GetExp()
    {
        if (!isLive)
            return;
        exp++;
        if (exp >= nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            exp -= nextExp[Mathf.Min(level, nextExp.Length - 1)];
            level++;
            if (damageBatchDepth > 0)
                pendingLevelUps++;
            else
                uiLevelUp.Show();
        }
    }

    public void BeginDamageBatch()
    {
        damageBatchDepth++;
    }

    public void EndDamageBatch()
    {
        damageBatchDepth--;
        if (damageBatchDepth == 0 && isLive)
            ShowPendingLevelUp();
    }

    public bool ShowPendingLevelUp()
    {
        if (pendingLevelUps == 0)
            return false;
        pendingLevelUps--;
        uiLevelUp.Show();
        return true;
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1f;
    }
}
